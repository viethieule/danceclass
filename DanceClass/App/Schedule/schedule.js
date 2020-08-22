const TIME_SLOTS = [
    { hours: 9, minutes: 0 },
    { hours: 10, minutes: 0 },
    { hours: 11, minutes: 0 },
    { hours: 12, minutes: 0 },
    { hours: 17, minutes: 0 },
    { hours: 18, minutes: 0 },
    { hours: 18, minutes: 30 },
    { hours: 19, minutes: 0 },
    { hours: 19, minutes: 35 },
    { hours: 20, minutes: 0 },
];

let m_currentDaysOfWeek = [];
let m_scheduleDetails = [];

let m_user = null;

$(async function () {
    collapseSideBar();
    initWeek();
    await initUser();

    registerEvent();
    renderUserRemainingSessions();
    await renderCalendar();
    initCreateScheduleForm();
});

async function initUser() {
    m_user = await userService.getCurrentUser();
}

function initWeek(currentDate) {
    currentDate = moment(currentDate || new Date());

    var weekStart = currentDate.clone().startOf('isoWeek');

    for (var i = 0; i <= 6; i++) {
        m_currentDaysOfWeek.push(moment(weekStart).add(i, 'days'));
    }
}

function renderUserRemainingSessions() {
    if (m_user && m_user.activePackage) {
        let remainingSessions = m_user.activePackage.remainingSessions;
        $('.calendar-remaining-sessions').html(formatRemainingSessions(remainingSessions));
    } else if (m_user && m_user.roleNames.includes("Member")) {
        $('.calendar-remaining-sessions').html(0);
    } else {
        $('.calendar-remaining-sessions-area').hide();
    }
}

function formatRemainingSessions(remainingSessions) {
    return remainingSessions < 10 && remainingSessions !== 0 ? "0" + remainingSessions : remainingSessions;
}

function registerEvent() {
    $('.btn-calendar-navigate').click(e => {
        console.log(e);
        if (e.target.id === 'prev') {
            m_currentDaysOfWeek = m_currentDaysOfWeek.map(day => day.subtract(7, 'days'));
        } else if (e.target.id === 'next') {
            m_currentDaysOfWeek = m_currentDaysOfWeek.map(day => day.add(7, 'days'));
        }
        renderCalendar();
    })

    $('#modal-register').on('show.bs.modal', function (event) {
        let div = $(event.relatedTarget);
        const id = div.data('id');

        let $modal = $(this);
        let modalTitle = $modal.find('.modal-title');
        let modalBodyInfo = $modal.find('.modal-body-info');
        let btnAction = $modal.find('#btn-action');

        modalBodyInfo.empty();
        btnAction.off('click');

        $('.modal-body-message').empty();
        $('.modal-body-remaining-sessions').html(m_user && m_user.activePackage ? m_user.activePackage.remainingSessions : 0);

        const scheduleDetail = m_scheduleDetails.find(x => x.id === parseInt(id));
        if (!scheduleDetail) {
            return;
        }

        const { currentUserRegistration } = scheduleDetail;
        if (scheduleDetail.isCurrentUserRegistered && currentUserRegistration && currentUserRegistration.status.value === 0) {
            modalTitle.text('Hủy đăng ký');
            modalBodyInfo.text('Bạn có chắc muốn hủy đăng ký?');
            btnAction.html('Hủy');
            btnAction.on('click', async function (e) {
                handleUnregisterScheduleClick(currentUserRegistration.id, $modal)
            });
        } else {
            modalTitle.text('Bạn có chắc chắn muốn đăng ký?');

            $('<p>').text('Lớp: ' + scheduleDetail.schedule.class.name).appendTo(modalBodyInfo);
            $('<p>').text('Bài: ' + scheduleDetail.schedule.song).appendTo(modalBodyInfo).appendTo(modalBodyInfo);
            $('<p>').text('Thời gian: ' + capitalizeFirstLetter(moment(scheduleDetail.date).locale('vi').format('dddd D/M'))).appendTo(modalBodyInfo);
            $('<p>').text('Địa điểm: ' + scheduleDetail.schedule.branch).appendTo(modalBodyInfo);

            btnAction.html('Đăng ký');
            btnAction.on('click', async function (e) {
                await handleRegisterScheduleClick(scheduleDetail, $modal)
            });
        }
    })

    $('#modal-manage').on('shown.bs.modal', function (event) {

        let div = $(event.relatedTarget);
        const id = parseInt(div.data('id'));

        const scheduleDetail = m_scheduleDetails.find(x => x.id === id);
        const { schedule, registrations, date, totalRegistered, sessionNo } = scheduleDetail;

        let $modal = $(this);

        let [hour, minute, ...rest] = schedule.startTime.split(':');
        let timeStart = capitalizeFirstLetter(moment(date).hours(parseInt(hour)).minute(parseInt(minute)).locale('vi').format('dddd D/M HH:mm'));
        $modal.find('.modal-title').text(schedule.class.name + ' - ' + timeStart);

        const { song, branch, sessions: totalSessions } = schedule;
        $('.session-general-info')
            .empty()
            .append(renderSessionInfoGroup('Bài múa', song))
            .append(renderSessionInfoGroup('Buổi', sessionNo + (totalSessions ? ' / ' + totalSessions : '')))
            .append(renderSessionInfoGroup('Thời gian', timeStart))
            .append(renderSessionInfoGroup('Địa điểm', branch))
            .append(renderSessionInfoGroup('Số học viên đăng ký', totalRegistered + ' / 20'));

        renderRegistrationList(registrations);

        $('.session-user-search-result').hide();
        $('.session-search-message').text('').hide();
        $('#session-user-search').val('');
        $('.session-add-registration button').off('click').on('click', async function (event) {
            let $addBtn = $(event.target);
            let $input = $addBtn.closest('div').find('input');
            let phoneNumber = $input.val();
            if (!phoneNumber) {
                return;
            }

            $addBtn.find('i').removeClass('fa fa-plus').addClass('fa fa-circle-o-notch fa-spin').prop('disabled', true);
            try {
                const user = await userService.get({ phoneNumber });
                $('.session-user-search-result').show();
                if (user) {
                    if (registrations.some(r => r.userId === user.id)) {
                        $('.session-search-message').text('Học viên ' + user.fullName + ' đã đăng ký!').show();
                        $('.session-user-search-result tbody').empty();
                        return;
                    }

                    let registerBtn = $('<button>', { class: 'btn btn-block btn-success btn-xs btn-label' })
                        .html('Đăng ký')
                        .on('click', { id }, async function (event) {
                            let $registerBtn = $(event.target);
                            $registerBtn
                                .prop('disabled', true)
                                .empty()
                                .append($('<i>', { class: 'fa fa-circle-o-notch fa-spin' }))
                            try {
                                const response = await registerSchedule(event.data.id, user.id);
                                if (response && response.registration) {
                                    response.registration.isModified = true;
                                    registrations.push(response.registration);

                                    // rerender
                                    renderRegistrationList(registrations);

                                    // hide search result
                                    $('.session-user-search-result').hide();
                                }
                            } catch (ex) {
                                console.log(ex);
                                $('.session-search-message').text(ex.responseJSON ? ex.responseJSON.ExceptionMessage : ex).show();
                            } finally {
                                // empty search result
                                $('.session-user-search-result tbody').empty();
                            }
                        });

                    let searchResult = $('<tr>')
                        .append($('<td>').text(user.fullName))
                        .append($('<td>').text(user.userName))
                        .append($('<td>').text(user.phoneNumber))
                        .append($('<td>').append(registerBtn));

                    $('.session-user-search-result tbody').empty().append(searchResult);
                } else {
                    $('.session-search-message').text('Không tìm thấy học viên!').show();
                    $('.session-user-search-result tbody').empty();
                }
            } catch (ex) {
                console.log(ex);
                $('.session-search-message').text(ex.responseJSON ? ex.responseJSON.ExceptionMessage : ex).show();
                $('.session-user-search-result tbody').empty();
            } finally {
                $addBtn.find('i')
                    .removeClass('fa fa-circle-o-notch fa-spin').addClass('fa fa-plus')
                    .prop('disabled', false);
            }
        })

        $('#modal-manage').off('hide.bs.hide.bs.modal').on('hide.bs.hide.bs.modal', async function (event) {
            if (registrations.some(r => r.isModified) || totalRegistered !== registrations.length) {
                await renderSchedule();
            }
        })
    });

    registerCreateScheduleModal();

    $('.session-registrations').slimscroll({
        distance: '5px'
    });
}

function renderSessionInfoGroup(label, value) {
    return $('<div>', { class: 'session-info-group' })
        .append($('<p>', { class: 'session-info-label' }).text(label))
        .append($('<p>', { class: 'session-info' }).text(value));
}

function renderRegistrationList(registrations) {
    let $registrationList = registrations.reduce((jObject, registration, index, arr) => {
        jObject = jObject.add(renderRegistrationRow(registration, index, registrations));
        return jObject;
    }, $());

    $('.session-registration-list').empty().append($registrationList);
}

function renderRegistrationRow(registration, index, registrations) {
    let tdNo = $('<td>').html(index + 1);
    let tdName = $('<td>').html(registration.user.fullName);
    let confirmBtn = $('<button>', { class: 'btn btn-success btn-xs btn-label' })
        .html('Đến lớp')
        .on('click', { registration }, handleConfirmRegistration);

    let cancelBtn = $('<button>', { class: 'btn btn-danger btn-xs btn-label' })
        .html('Hủy')
        .on('click', { registration, registrations }, handleCancelRegistration);

    if (registration.status && registration.status.value === 1) {
        confirmBtn
            .prop('disabled', true)
            .off('click')
            .prepend($('<i>', { class: 'fa fa-check' }))

        cancelBtn.prop('disabled', true).hide();
    }

    let tdActionBtns = $('<td>').append(confirmBtn).append(cancelBtn);

    return $('<tr>').append(tdNo).append(tdName).append(tdActionBtns)
}

async function handleConfirmRegistration(event) {
    let $confirmBtn = $(event.target);
    $confirmBtn
        .prop('disabled', true)
        .empty()
        .append($('<li>', { class: 'fa fa-circle-o-notch fa-spin' }));

    let $cancelBtn = $confirmBtn.closest('td').find('.btn-danger');
    $cancelBtn.prop('disabled', true).hide();

    let { registration } = event.data;
    try {
        await confirmRegistration(registration.id);

        registration.status.value = 1;
        registration.status.name = 'Đến lớp';

        $confirmBtn
            .empty()
            .append($('<i>', { class: 'fa fa-check' }))
            .append('Đến lớp')
            .off('click');

    } catch (ex) {
        console.log(ex);
        $cancelBtn.prop('disabled', false).show();
        $confirmBtn
            .empty()
            .append('Đăng ký')
            .prop('disabled', false);

        $cancelBtn.closest('td').append(ex.responseJSON ? ex.responseJSON.ExceptionMessage : 'Đã có lỗi xảy ra');
    }
}

async function handleCancelRegistration(event) {
    let $cancelBtn = $(event.target);
    $cancelBtn
        .prop('disabled', true)
        .empty()
        .append($('<i>', { class: 'fa fa-circle-o-notch fa-spin' }));

    let $confirmBtn = $cancelBtn.closest('td').find('.btn-success');
    $confirmBtn.prop('disabled', true).hide();

    let { registration, registrations } = event.data;
    try {
        await unregisterSchedule(registration.id);

        let index = registrations.findIndex(r => r.id === registration.id);
        registrations.splice(index, 1);
        $cancelBtn.closest('tr').remove();
    } catch (ex) {
        console.log(ex);
        $confirmBtn.prop('disabled', false).show();
        $cancelBtn
            .empty()
            .append('Hủy')
            .prop('disabled', false);

        $cancelBtn.closest('td').append(ex.responseJSON ? ex.responseJSON.ExceptionMessage : 'Đã có lỗi xảy ra');
    }
}

async function handleUnregisterScheduleClick(registrationId, $modal) {
    try {
        await unregisterSchedule(registrationId);
        $('.modal-body-message').css('color', 'green').text('Hủy thành công');
        setTimeout(async function () {
            $modal.modal('hide');
            updateUserRemainingSessions(false);
            await renderSchedule();
        }, 1000);
    } catch (ex) {
        console.log(ex);
        $('.modal-body-message').css('color', 'red').text(ex.responseJSON.ExceptionMessage);
    }
}

async function handleRegisterScheduleClick(scheduleDetail, $modal) {
    try {
        const rs = await registerSchedule(scheduleDetail.id, m_user.id);
        if (rs && rs.registration) {
            $('.modal-body-message').css('color', 'green').text('Đăng ký thành công');
            setTimeout(async function () {
                $modal.modal('hide');
                updateUserRemainingSessions(true);
                await renderSchedule();
            }, 1000);
        }
    } catch (ex) {
        console.log(ex);
        $('.modal-body-message').css('color', 'red').text(ex.responseJSON.ExceptionMessage);
    }
}

function updateUserRemainingSessions(isRegistration) {
    if (m_user && m_user.activePackage) {
        if (isRegistration) {
            m_user.activePackage.remainingSessions--;
        }
        else {
            m_user.activePackage.remainingSessions++;
        }
        renderUserRemainingSessions();
    }
}

async function renderCalendar() {
    // render days of current week header
    renderDaysOfWeek();
    // render schedule data
    await renderSchedule();
}

function renderDaysOfWeek() {
    $('.calendar-control-current-week').html(
        `${m_currentDaysOfWeek[0].locale('vi').format('DD/MM')} - ${m_currentDaysOfWeek[6].locale('vi').format('DD/MM')}`
    );
    $('#calendarHead').empty();

    let $tr = $('<tr>').append($('<th>'));
    m_currentDaysOfWeek
        .forEach(day => {
            let dayLocale = capitalizeFirstLetter(day.locale('vi').format('dddd D/M'));
            let $th = $('<th>').text(dayLocale);
            if (day.isSame(new Date(), 'day')) {
                $th.css('background-color', '#ECF0F5');
            }
            $th.appendTo($tr);
        });

    $('#calendarHead').append($tr);
}

async function renderSchedule() {
    $('#calendarBody').empty();

    const eventsByTime = await getSchedule();

    eventsByTime.forEach((eventGroup) => {
        let tr = $('<tr></tr>');
        const { hours, minutes } = eventGroup;
        $('<td></td>')
            .html(
                `${formatHhMm(hours, minutes)} <br />- ${formatHhMm(hours + 1, minutes)}`
            )
            .css('padding', '6px 0px')
            .appendTo(tr);

        m_currentDaysOfWeek.forEach(date => {
            let targetedDate = date.clone();
            targetedDate.hour(hours).minute(minutes);

            let tdEvents = $('<td></td>');
            let events = eventGroup.events.filter(e => (new Date(e.date)).getDay() === date.day());
            if (events && events.length > 0) {
                let isPast = targetedDate.isBefore(new Date(), 'second');
                events
                    .reduce((jObject, event) => {
                        jObject = jObject.add(renderEventTag(event, isPast));
                        return jObject;
                    }, $())
                    .appendTo(tdEvents);
            } else {
                if (date.isSame(new Date(), 'day')) {
                    tdEvents.css('background-color', '#ECF0F5');
                }

                if (userService.isAdmin()) {
                    tdEvents
                        .attr('data-toggle', 'modal')
                        .attr('data-target', '#modal-create-schedule')
                        .data('date', targetedDate)
                        .hover(function () {
                            $(this)
                                .addClass('cell-admin-add-schedule')
                                .html('Tạo<br />lịch học')
                        }, function () {
                            $(this)
                                .removeClass('cell-admin-add-schedule')
                                .html('')
                        })
                }
            }

            tdEvents.appendTo(tr);
        });

        tr.appendTo('#calendarBody');
    });
}

function renderEventTag(event, isPast) {
    const { schedule } = event;
    const isMember = userService.isMember();
    const isAdmin = userService.isAdmin();

    var div = $('<div></div>', {
        'class': 'mistake-event mistake-event-' + schedule.branch.toLowerCase(),
        'data-toggle': 'modal',
        'data-target': isAdmin ? '#modal-manage' : isMember ? '#modal-register' : '',
        'data-id': event.id
    });

    div.hover(function () { $(this).css('cursor', 'pointer') });

    let $class = $('<p></p>', {
        class: 'mistake-event-class',
    })
        .text(schedule.class.name)
        .appendTo(div);

    if (event.sessionNo === 1) {
        $class.prepend($('<small>', { class: 'label mistake-event-label-new' }).html('new'))
    }

    $('<p></p>', {
        class: 'mistake-event-song',
    })
        .html(schedule.song ? ('&nbsp; ' + schedule.song) : '&nbsp; <i>(Đang chờ cập nhật...)</i>')
        .prepend($('<li>', { class: 'fa fa-music' }))
        .appendTo(div);

    var infoDiv = $('<div></div>', {
        class: 'mistake-event-info-container'
    });

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .html(`&nbsp; ${event.sessionNo}${schedule.sessions ? '/' + schedule.sessions : ''}`)
        .prepend($('<li>', { class: 'fa fa-calendar' }))
        .appendTo(infoDiv);

    if (isAdmin) {
        $('<span></span>', {
            class: 'mistake-event-info',
        })
            .html(`&nbsp; ${event.totalRegistered}/20`)
            .prepend($('<li>', { class: 'fa fa-user' }))
            .appendTo(infoDiv);
    }

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .html('&nbsp; ' + schedule.branch)
        .prepend($('<li>', { class: 'fa fa-map-marker' }))
        .appendTo(infoDiv);

    infoDiv.appendTo(div);

    if (isMember) {
        let btnLabel = '';
        let btnClass = '';

        let { currentUserRegistration } = event;
        let isRegistered = event.isCurrentUserRegistered;

        if (isRegistered && currentUserRegistration && currentUserRegistration.status.value === 0) {
            btnLabel = 'Hủy đăng ký';
            btnClass = 'btn-danger';
        } else if (isRegistered && currentUserRegistration && currentUserRegistration.status.value === 1) {
            btnLabel = 'Đã đến lớp';
            btnClass = 'btn-primary';
            div.removeAttr('data-toggle').removeAttr('data-target');
            div.off('mouseenter mouseleave');
        } else {
            btnLabel = 'Đăng ký';
            btnClass = 'btn-success';
        }

        if (isPast) {
            div.removeAttr('data-toggle').removeAttr('data-target');
            div.off('mouseenter mouseleave');
        }

        let isDisabled = isPast && !(isRegistered && currentUserRegistration && currentUserRegistration.status.value === 1);
        $('<div>', { class: 'mistake-event-action' })
            .append($('<button>', { class: 'btn btn-xs ' + btnClass }).html(btnLabel).prop('disabled', isDisabled))
            .appendTo(div);
    }

    return div;
}

async function getSchedule() {
    try {
        const data = await ajaxSchedule(m_currentDaysOfWeek[0].toDate());
        console.log(data);

        if (data && data.scheduleDetails) {
            let timeSlotsTemplate = userService.isAdmin() ? TIME_SLOTS.map(s => Object.assign({}, s)) : [];
            timeSlotsTemplate.forEach(s => s.events = []);

            m_scheduleDetails = data.scheduleDetails;
            const eventsByTime = m_scheduleDetails.reduce((result, ele) => {
                let [hours, minutes, ...rest] = ele.schedule.startTime.split(":");

                hours = parseInt(hours);
                minutes = parseInt(minutes);

                var eventGroup = result.find(
                    (r) => r.hours === hours && r.minutes === minutes
                );

                if (!eventGroup) {
                    result.push({
                        hours,
                        minutes,
                        events: [ele],
                    });
                } else {
                    eventGroup.events.push(ele);
                }

                return result;
            }, timeSlotsTemplate).sort(hourMinuteComparer);

            console.log(eventsByTime);
            return eventsByTime;
        }
    } catch (err) {
        console.log(err);
        return [];
    }
}

function hourMinuteComparer(t1, t2) {
    const { hours: h1, minutes: m1 } = t1;
    const { hours: h2, minutes: m2 } = t2;
    return h1 > h2 ? 1 : h1 < h2 ? -1 : m1 > m2 ? 1 : m1 < m2 ? -1 : 0;
}

async function confirmRegistration(registrationId) {
    return $.ajax({
        method: 'PUT',
        async: true,
        url: '/api/registration/confirmAttendance/' + registrationId
    });
}

async function registerSchedule(scheduleDetailId, userId) {
    return $.ajax({
        method: 'POST',
        data: {
            registration: {
                scheduleDetailId,
                userId
            }
        },
        async: true,
        url: '/api/registration/create',
    });
}

async function unregisterSchedule(id) {
    return $.ajax({
        method: 'POST',
        data: { registrationId: id },
        async: true,
        url: '/api/registration/cancel',
    });
}

async function ajaxSchedule(weekStart) {
    return $.ajax({
        method: 'POST',
        data: { start: weekStart.toISOString() },
        async: true,
        url: '/api/schedule/getdetail',
    });
}
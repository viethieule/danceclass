let m_currentDaysOfWeek = [];
let m_scheduleDetails = [];

let m_user = null;

$(async function () {
    registerEvent();
    initWeek();
    await initUser();
    renderUserRemainingSessions();
    await renderCalendar();
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
    if (m_user && m_user.ActivePackage) {
        let remainingSessions = m_user.ActivePackage.RemainingSessions;
        $('.calendar-remaining-sessions').html(formatRemainingSessions(remainingSessions));
    } else if (m_user && m_user.RoleNames.includes("Member")) {
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
        const id = div.data('id') // Extract info from data-* attributes
        // If necessary, you could initiate an AJAX request here (and then do the updating in a callback).
        // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.

        let $modal = $(this);
        let modalTitle = $modal.find('.modal-title');
        let modalBodyInfo = $modal.find('.modal-body-info');
        let btnAction = $modal.find('#btn-action');

        modalBodyInfo.empty();
        btnAction.off('click');

        $('.modal-body-message').empty();
        $('.modal-body-remaining-sessions').html(m_user && m_user.ActivePackage ? m_user.ActivePackage.RemainingSessions : 0);

        const scheduleDetail = m_scheduleDetails.find(x => x.Id === parseInt(id));
        if (scheduleDetail.IsCurrentUserRegistered) {
            modalTitle.text('Hủy đăng ký');
            modalBodyInfo.text('Bạn có chắc muốn hủy đăng ký?');
            btnAction.html('Hủy');
            btnAction.on('click', async function (e) {
                handleUnregisterScheduleClick(scheduleDetail.CurrentUserRegistration.Id, $modal)
            });
        } else {
            modalTitle.text('Bạn có chắc chắn muốn đăng ký?');

            $('<p>').text('Lớp: ' + scheduleDetail.Schedule.Class.Name).appendTo(modalBodyInfo);
            $('<p>').text('Bài: ' + scheduleDetail.Schedule.Song).appendTo(modalBodyInfo).appendTo(modalBodyInfo);
            $('<p>').text('Thời gian: ' + capitalizeFirstLetter(moment(scheduleDetail.Date).locale('vi').format('dddd D/M'))).appendTo(modalBodyInfo);
            $('<p>').text('Địa điểm: ' + scheduleDetail.Schedule.Branch).appendTo(modalBodyInfo);

            btnAction.html('Đăng ký');
            btnAction.on('click', async function (e) {
                await handleRegisterScheduleClick(scheduleDetail, $modal)
            });
        }
    })

    $('#modal-manage').on('shown.bs.modal', function (event) {

        let div = $(event.relatedTarget);
        const id = div.data('id');

        const scheduleDetail = m_scheduleDetails.find(x => x.Id === parseInt(id));
        const { Schedule: schedule, Registrations: registrations, Date: date, TotalRegistered: totalRegistered, SessionNo: sessionNo } = scheduleDetail;

        let $modal = $(this);

        let [hour, minute, ...rest] = schedule.StartTime.split(':');
        let timeStart = capitalizeFirstLetter(moment(date).hours(parseInt(hour)).minute(parseInt(minute)).locale('vi').format('dddd D/M HH:mm'));
        $modal.find('.modal-title').text(schedule.Class.Name + ' - ' + timeStart);

        const { Song: song, Branch: branch, Sessions: totalSessions } = schedule;
        $('.session-general-info')
            .empty()
            .append(renderSessionInfoGroup('Bài múa', song))
            .append(renderSessionInfoGroup('Buổi', sessionNo + ' / ' + totalSessions))
            .append(renderSessionInfoGroup('Thời gian', timeStart))
            .append(renderSessionInfoGroup('Địa điểm', branch))
            .append(renderSessionInfoGroup('Số học viên đăng ký', totalRegistered + ' / 20'));

        let $registrationList = registrations.reduce((jObject, registration, index) => {
            jObject = jObject.add(renderRegistrationRow(registration, index));
            return jObject;
        }, $());

        $('.session-registration-list').empty().append($registrationList);

        $('#modal-manage').off('hide.bs.hide.bs.modal').on('hide.bs.hide.bs.modal', async function (event) {
            if (registrations.some(r => r.isModified)) {
                await renderCalendar();
            }
        })
    });

    $('.session-registrations').slimscroll({
        distance: '5px'
    });
}

function renderSessionInfoGroup(label, value) {
    return $('<div>', { class: 'session-info-group' })
        .append($('<p>', { class: 'session-info-label' }).text(label))
        .append($('<p>', { class: 'session-info' }).text(value));
}

function renderRegistrationRow(registration, index) {
    let tdNo = $('<td>').html(index + 1);
    let tdName = $('<td>').html(registration.User.FullName);
    let confirmBtn = $('<button>', { class: 'btn btn-success btn-xs btn-label' })
        .html('Đến lớp')
        .on('click', { registration }, handleConfirmRegistration);

    let cancelBtn = $('<button>', { class: 'btn btn-danger btn-xs btn-label' })
        .html('Hủy')
        .on('click', { registration }, handleCancelRegistration);

    if (registration.Status && registration.Status.Value === 1) {
        confirmBtn
            .prop('disabled', true)
            .off('click')
            .prepend($('<i>', { class: 'fa fa-check' }))

        cancelBtn.prop('disabled', true).hide();
    } else if (registration.Status && registration.Status.Value === 2) {
        confirmBtn.prop('disabled', true).hide();

        cancelBtn
            .html('Nghỉ')
            .off('click')
            .prop('disabled', true)
            .prepend($('<i>', { class: 'fa fa-times' }))
    }

    let tdActionBtns = $('<td>').append(confirmBtn).append(cancelBtn);

    return $('<tr>').append(tdNo).append(tdName).append(tdActionBtns)
}

async function handleConfirmRegistration(event) {
    try {
        let $confirmBtn = $(event.target);
        $confirmBtn
            .prop('disabled', true)
            .empty()
            .append($('<li>', { class: 'fa fa-circle-o-notch fa-spin' }));

        let $cancelBtn = $confirmBtn.closest('td').find('.btn-danger');
        $cancelBtn.prop('disabled', true).hide();

        let { registration } = event.data;
        await confirmRegistration(event.data.registration.Id);

        registration.Status.Value = 1;
        registration.Status.Name = 'Đến lớp';

        $confirmBtn
            .empty()
            .append($('<i>', { class: 'fa fa-check' }))
            .append('Đến lớp')
            .off('click');

    } catch (ex) {
        console.log(ex);
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

    let { registration } = event.data;
    try {
        await unregisterSchedule(registration.Id);

        registration.Status.Value = 2;
        registration.Status.Name = 'Nghỉ';
        registration.isModified = true;

        $cancelBtn
            .empty()
            .append($('<i>', { class: 'fa fa-times' }))
            .append('Nghỉ')
            .off('click');
    } catch (ex) {
        console.log(ex);
        $confirmBtn.prop('disabled', false).show();
        $cancelBtn
            .empty()
            .append('Hủy')
            .prop('disabled', false);

        $cancelBtn.closest('td').append('  Đã có lỗi xảy ra!');
    }

}

async function handleUnregisterScheduleClick(registrationId, $modal) {
    try {
        await unregisterSchedule(registrationId, true);
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
        const rs = await registerSchedule(scheduleDetail.Id);
        if (rs && rs.Registration) {
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
    if (m_user && m_user.ActivePackage) {
        if (isRegistration) {
            m_user.ActivePackage.RemainingSessions--;
        }
        else {
            m_user.ActivePackage.RemainingSessions++;
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

    var weekTableData = m_currentDaysOfWeek
        .map(day => {
            var dayLocale = capitalizeFirstLetter(day.locale('vi').format('dddd D/M'));
            return `<th>${dayLocale}</th>`;
        })
        .join('');

    var weekTableHead = `<tr><th></th>${weekTableData}</th>`;
    $('#calendarHead').append(weekTableHead);
}

async function renderSchedule() {
    $('#calendarBody').empty();

    const eventsByTime = await getSchedule();

    eventsByTime.forEach((eventGroup) => {
        let tr = $('<tr></tr>');
        let tdTime = $('<td></td>')
            .html(
                `${formatHhMm(eventGroup.hours, eventGroup.minutes)} <br />- ${formatHhMm(eventGroup.hours + 1, eventGroup.minutes)}`
            )
            .appendTo(tr);

        m_currentDaysOfWeek.forEach(date => {
            let tdEvents = $('<td></td>');
            let events = eventGroup.events.filter(e => (new Date(e.Date)).getDay() === date.day())
            if (events && events.length > 0) {
                events
                    .reduce((jObject, event) => {
                        jObject = jObject.add(renderEventTag(event));
                        return jObject;
                    }, $())
                    .appendTo(tdEvents);
            } else {
                if (userService.isAdmin()) {
                    tdEvents
                        .on('click', function (event) { console.log(event) })
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

function renderEventTag(event) {
    const { Schedule: schedule, Registrations: registrations } = event;

    var div = $('<div></div>', {
        'class': 'mistake-event mistake-event-' + schedule.Branch.toLowerCase(),
        'data-toggle': 'modal',
        'data-target': userService.isAdmin() ? '#modal-manage' : userService.isMember() ? '#modal-register' : '',
        'data-id': event.Id
    });

    div.hover(function () { $(this).css('cursor', 'pointer') });

    $('<p></p>', {
        class: 'mistake-event-class',
    })
        .text(schedule.Class.Name)
        .appendTo(div);

    $('<p></p>', {
        class: 'mistake-event-song',
    })
        .text(schedule.Song)
        .appendTo(div);

    var infoDiv = $('<div></div>', {
        class: 'mistake-event-info-container'
    });

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .text(`${event.SessionNo}/${schedule.Sessions}`)
        .appendTo(infoDiv);

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .text(`${event.TotalRegistered}/20`)
        .appendTo(infoDiv);

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .text(schedule.Branch)
        .appendTo(infoDiv);

    infoDiv.appendTo(div);

    if (userService.isMember()) {
        let isRegistered = event.IsCurrentUserRegistered && event.CurrentUserRegistration.Status.Value === 0;
        let action = isRegistered ? 'Hủy đăng ký' : 'Đăng ký';
        let btnClass = isRegistered ? 'btn-danger' : 'btn-success';
        $('<div>', { class: 'mistake-event-action' })
            .append($('<button>', { class: 'btn btn-xs ' + btnClass }).html(action))
            .appendTo(div);
    }

    return div;
}

async function getSchedule() {
    try {
        const data = await ajaxSchedule(m_currentDaysOfWeek[0].toDate());
        console.log(data);

        if (data && data.ScheduleDetails) {
            m_scheduleDetails = data.ScheduleDetails;
            const eventsByTime = m_scheduleDetails.reduce((result, ele) => {
                let [hours, minutes, ...rest] = ele.Schedule.StartTime.split(":");

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
            }, []).sort(compareTime);

            console.log(eventsByTime);
            return eventsByTime;
        }
    } catch (err) {
        console.log(err);
        return [];
    }
}

function formatHhMm(hours, minutes) {
    const formatLeadingZero = (timePart) =>
        `${
        timePart.toString().length === 1
            ? '0' + timePart
            : timePart.toString()
        }`;
    return `${formatLeadingZero(hours)}:${formatLeadingZero(minutes)}`;
}

function compareTime(t1, t2) {
    const { hours: h1, minutes: m1 } = t1;
    const { hours: h2, minutes: m2 } = t2;
    return h1 > h2 ? 1 : h1 < h2 ? -1 : m1 > m2 ? 1 : m1 < m2 ? -1 : 0;
}

function getCurrentRecurrenceNumber(startRecur, currentDate, daysOfWeek) {
    var daysPerWeek = daysOfWeek.length;
    var numberOfSessionGoneBy =
        Math.floor(moment(currentDate).diff(moment(startRecur), 'days') / 7) *
        daysPerWeek;

    var currentNumberOfSessionInWeek =
        daysOfWeek.indexOf(currentDate.getDay()) + 1;
    return numberOfSessionGoneBy + currentNumberOfSessionInWeek;
}

function capitalizeFirstLetter(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
}

async function confirmRegistration(registrationId) {
    return $.ajax({
        method: 'PUT',
        async: true,
        url: '/api/registration/confirmAttendance/' + registrationId
    });
}

async function registerSchedule(scheduleDetailId) {
    return $.ajax({
        method: 'POST',
        data: {
            registration: { scheduleDetailId }
        },
        async: true,
        url: '/api/registration/create',
    });
}

async function unregisterSchedule(id, isDelete) {
    return $.ajax({
        method: 'POST',
        data: {
            registrationId: id,
            isDelete
        },
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
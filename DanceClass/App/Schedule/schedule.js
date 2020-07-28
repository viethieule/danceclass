let m_currentDaysOfWeek = [];
let m_scheduleDetails = [];

let m_user = null;

$(async function () {
    await initData();
    initWeek();
    await renderCalendar();
    renderUserRemainingSessions();
    registerEvent();
});

async function initData() {
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

        let modal = $(this);
        let modalTitle = modal.find('.modal-title');
        let modalBodyInfo = modal.find('.modal-body-info');
        let btnAction = modal.find('#btn-action');

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
                handleUnregisterScheduleClick(scheduleDetail.CurrentUserRegistration.Id, modal)
            });
        } else {
            modalTitle.text('Bạn có chắc chắn muốn đăng ký?');

            $('<p>').text('Lớp: ' + scheduleDetail.Schedule.Class.Name).appendTo(modalBodyInfo);
            $('<p>').text('Bài: ' + scheduleDetail.Schedule.Song).appendTo(modalBodyInfo).appendTo(modalBodyInfo);
            $('<p>').text('Thời gian: ' + capitalizeFirstLetter(moment(scheduleDetail.Date).locale('vi').format('dddd D/M'))).appendTo(modalBodyInfo);
            $('<p>').text('Địa điểm: ' + scheduleDetail.Schedule.Branch).appendTo(modalBodyInfo);

            btnAction.html('Đăng ký');
            btnAction.on('click', async function (e) {
                await handleRegisterScheduleClick(scheduleDetail, modal)
            });
        }
    })
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
        'data-target': '#modal-register',
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
        .text(`${registrations.length}/20`)
        .appendTo(infoDiv);

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .text(schedule.Branch)
        .appendTo(infoDiv);

    infoDiv.appendTo(div);

    if (m_user && m_user.RoleNames.includes("Member")) {
        let action = event.IsCurrentUserRegistered ? 'Hủy đăng ký' : 'Đăng ký';
        let btnClass = event.IsCurrentUserRegistered ? 'btn-danger' : 'btn-success';
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

async function unregisterSchedule(id) {
    return $.ajax({
        method: 'POST',
        data: id.toString(),
        async: true,
        url: '/api/registration/cancel',
        contentType: 'application/json',
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
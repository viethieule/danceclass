var m_currentDaysOfWeek = [];

$(async function () {
    initWeek();
    await renderCalendar();
    registerEvent();
    const user = await getCurrentUser();
    console.log(user);
});

function initWeek(currentDate) {
    currentDate = moment(currentDate || new Date());

    var weekStart = currentDate.clone().startOf('isoWeek');

    for (var i = 0; i <= 6; i++) {
        m_currentDaysOfWeek.push(moment(weekStart).add(i, 'days'));
    }
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
}

async function renderCalendar() {
    $('#calendarHead').empty();
    $('#calendarBody').empty();

    $('.calendar-control-current-week').html(
        `${m_currentDaysOfWeek[0].locale('vi').format('DD/MM')} - ${m_currentDaysOfWeek[6].locale('vi').format('DD/MM')}`
    );

    // render days of current week header
    renderDaysOfWeek();
    // render schedule data
    await renderSchedule();
}

function renderDaysOfWeek() {
    var weekTableData = m_currentDaysOfWeek
        .map((day) => {
            var dayLocale = capitalizeFirstLetter(
                day.locale('vi').format('dddd D/M')
            );
            return `<th>${dayLocale}</th>`;
        })
        .join('');

    var weekTableHead = `<tr><th></th>${weekTableData}</th>`;
    $('#calendarHead').append(weekTableHead);
}

async function renderSchedule() {
    const eventsByTime = await getSchedule();

    eventsByTime.forEach((eventGroup) => {
        let tr = $('<tr></tr>');
        let tdTime = $('<td></td>')
            .html(
                `${formatHhMm(eventGroup.hours, eventGroup.minutes)} <br />- ${formatHhMm(eventGroup.hours + 1, eventGroup.minutes)}`
            )
            .appendTo(tr);

        m_currentDaysOfWeek.forEach((date) => {
            let tdEvents = $('<td></td>');
            let divEvents = eventGroup.events
                .reduce((jObject, event) => {
                    if (date.isBefore(moment(dateRevive(event.OpeningDate)), 'day')) {
                        return jObject;
                    }
                    if (event.DaysPerWeek.indexOf(date.day().toString()) !== -1) {
                        jObject = jObject.add(renderEventTag(event, date.toDate()));
                    }
                    return jObject;
                }, $())
                .appendTo(tdEvents);

            tdEvents.appendTo(tr);
        });

        tr.appendTo('#calendarBody');
    });
}

function renderEventTag(event, dateToRender) {
    var div = $('<div></div>', {
        class: 'mistake-event mistake-event-' + event.Branch.toLowerCase(),
    });

    $('<p></p>', {
        class: 'mistake-event-class',
    })
        .text(event.Class.Name)
        .appendTo(div);

    $('<p></p>', {
        class: 'mistake-event-song',
    })
        .text(event.Song)
        .appendTo(div);

    var infoDiv = $('<div></div>', {
        class: 'mistake-event-info-container'
    });

    var session = getCurrentRecurrenceNumber(
        dateRevive(event.OpeningDate),
        dateToRender,
        event.DaysPerWeek
    );
    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .text(`${session}/${event.Sessions}`)
        .appendTo(infoDiv);

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .text('14/20')
        .appendTo(infoDiv);

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .text(event.Branch)
        .appendTo(infoDiv);

    infoDiv.appendTo(div);
    return div;
}

async function getSchedule() {
    try {
        const data = await ajaxSchedule(m_currentDaysOfWeek[0].toDate());
        console.log(data);

        if (data && data.Schedules) {
            const eventsByTime = data.Schedules.reduce((result, ele) => {
                const { Hours: hours, Minutes: minutes } = ele.StartTime;

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

function dateRevive(jsonDate) {
    var result = /\/Date\((\d*)\)\//.exec(jsonDate);
    if (result) {
        return new Date(+result[1]);
    }
    return value;
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

async function ajaxSchedule(weekStart) {
    debugger;
    return $.ajax({
        method: 'POST',
        data: { start: weekStart.toISOString() },
        async: true,
        url: '/Services/Schedule/Get',
    });
}
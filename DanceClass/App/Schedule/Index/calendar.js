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

function CalendarManager() {
    var _self = this;

    if (window.screen.width >= 1024) {
        CalendarDesktopManager.call(this);
    } else {
        CalendarMobileManager.call(this);
    }

    this.initCalendar = function () {
        initWeek();

        this.renderUserRemainingSessions();
        renderCalendar();

        registerEvent();
    }

    this.renderUserRemainingSessions = function () {
        let user = _self.currentUser;
        if (user && user.activePackage) {
            let remainingSessions = user.activePackage.remainingSessions;
            $('.calendar-remaining-sessions').html(formatRemainingSessions(remainingSessions));
        } else if (user && user.roleNames.includes("Member")) {
            $('.calendar-remaining-sessions').html(0);
        } else {
            $('.calendar-remaining-sessions-area').hide();
        }
    }

    this.renderSchedule = async function () {
        $('#calendarBody').empty();

        const eventsByTime = await getSchedule();

        eventsByTime.forEach(function(eventGroup) {
            let tr = $('<tr></tr>');
            const { hours, minutes } = eventGroup;
            $('<td></td>')
                .html(`${Utils.formatHhMm(hours, minutes)} <br />- ${Utils.formatHhMm(hours + 1, minutes)}`)
                .css('padding', '6px 0px')
                .appendTo(tr);

            _self.currentDaysOfWeek.forEach(date => {
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

                    if (UserService.isAdmin()) {
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

    function formatRemainingSessions(remainingSessions) {
        return remainingSessions < 10 && remainingSessions !== 0 ? "0" + remainingSessions : remainingSessions;
    }

    function initWeek(currentDate) {
        currentDate = moment(currentDate || new Date());

        var weekStart = currentDate.clone().startOf('isoWeek');

        for (var i = 0; i <= 6; i++) {
            _self.currentDaysOfWeek.push(moment(weekStart).add(i, 'days'));
        }
    }

    function renderCalendar() {
        // render days of current week header
        renderDaysOfWeek();
        // render schedule data
        _self.renderSchedule();
    }

    function renderDaysOfWeek() {
        var currentDaysOfWeek = _self.currentDaysOfWeek;
        $('.calendar-control-current-week').html(
            `${currentDaysOfWeek[0].locale('vi').format('DD/MM')} - ${currentDaysOfWeek[6].locale('vi').format('DD/MM')}`
        );
        $('#calendarHead').empty();

        let $tr = $('<tr>').append($('<th>'));
        currentDaysOfWeek
            .forEach(day => {
                let dayLocale = Utils.capitalizeFirstLetter(day.locale('vi').format('dddd D/M'));
                let $th = $('<th>').text(dayLocale);
                if (day.isSame(new Date(), 'day')) {
                    $th.css('background-color', '#ECF0F5');
                }
                $th.appendTo($tr);
            });

        $('#calendarHead').append($tr);
    }

    function renderEventTag(event, isPast) {
        const { schedule } = event;
        const isMember = UserService.isMember();
        const isAdmin = UserService.isAdmin();

        var div = $('<div></div>', {
            'class': 'mistake-event mistake-event-' + schedule.branch.toLowerCase(),
            'data-toggle': 'modal',
            'data-target': isAdmin ? '#modal-manage' : isMember ? '#modal-register' : '',
            'data-id': event.id
        });

        div.hover(function () { $(this).css('cursor', 'pointer') });

        let $class = $('<p></p>', { class: 'mistake-event-class' })
            .text(schedule.class.name)
            .appendTo(div);

        if (event.sessionNo === 1) {
            $class.prepend(
                $('<small>', { class: 'label mistake-event-label-new' })
                    .html('new')
            )
        }

        $('<p></p>', { class: 'mistake-event-song' })
            .html(schedule.song ? ('&nbsp; ' + schedule.song) : '&nbsp; <i>(Đang chờ cập nhật...)</i>')
            .prepend($('<li>', { class: 'fa fa-music' }))
            .appendTo(div);

        var infoDiv = $('<div></div>', { class: 'mistake-event-info-container' });

        $('<span></span>', { class: 'mistake-event-info' })
            .html(`&nbsp; ${event.sessionNo}${schedule.sessions ? '/' + schedule.sessions : ''}`)
            .prepend($('<li>', { class: 'fa fa-calendar' }))
            .appendTo(infoDiv);

        if (isAdmin) {
            $('<span></span>', { class: 'mistake-event-info' })
                .html(`&nbsp; ${event.totalRegistered}/20`)
                .prepend($('<li>', { class: 'fa fa-user' }))
                .appendTo(infoDiv);
        }

        $('<span></span>', { class: 'mistake-event-info' })
            .html('&nbsp; ' + schedule.branch)
            .prepend($('<li>', { class: 'fa fa-map-marker' }))
            .appendTo(infoDiv);

        infoDiv.appendTo(div);

        if (isMember) {
            let btnLabel = '';
            let btnClass = '';

            let { currentUserRegistration } = event;
            let isRegistered = event.isCurrentUserRegistered && currentUserRegistration && currentUserRegistration.status.value === REGISTRATION_STATUS.REGISTERED;
            let isAttended = event.isCurrentUserRegistered && currentUserRegistration && currentUserRegistration.status.value === REGISTRATION_STATUS.ATTENDED;

            if (isRegistered) {
                btnLabel = 'Hủy đăng ký';
                btnClass = 'btn-danger';
            } else if (isAttended) {
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

            let isDisabled = isPast && !isAttended;
            $('<div>', { class: 'mistake-event-action' })
                .append($('<button>', { class: 'btn btn-xs ' + btnClass }).html(btnLabel).prop('disabled', isDisabled))
                .appendTo(div);
        }

        return div;
    }

    async function getSchedule() {
        try {
            const data = await ApiService.post('/api/schedule/getdetail', {
                start: _self.currentDaysOfWeek[0].toDate().toISOString()
            });

            if (data && data.scheduleDetails) {
                let timeSlotsTemplate = UserService.isAdmin() ? TIME_SLOTS.map(s => Object.assign({}, s)) : [];
                timeSlotsTemplate.forEach(s => s.events = []);

                _self.scheduleDetails = data.scheduleDetails;
                const eventsByTime = _self.scheduleDetails.reduce((result, ele) => {
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

    function registerEvent() {
        $('.btn-calendar-navigate').click(function (e) {
            if (e.target.id === 'prev') {
                _self.currentDaysOfWeek = _self.currentDaysOfWeek.map(day => day.subtract(7, 'days'));
            } else if (e.target.id === 'next') {
                _self.currentDaysOfWeek = _self.currentDaysOfWeek.map(day => day.add(7, 'days'));
            }
            renderCalendar();
        });
    }
}

function CalendarDesktopManager() {

}

function CalendarMobileManager() {

}
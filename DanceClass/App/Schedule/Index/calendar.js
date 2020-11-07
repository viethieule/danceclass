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

    this.selectedDay = null;
    this.selectedDayIndex = null;
    this.singleDayMode = false;

    this.initCalendar = function () {
        initWeek();

        this.renderUserRemainingSessions();
        renderCalendar();

        registerEvent();
        registerDeviceEvent();
    }

    this.renderUserRemainingSessions = function () {
        let user = _self.currentUser;
        if (user && user.membership) {
            let remainingSessions = user.membership.remainingSessions;
            $('.calendar-remaining-sessions').html(formatRemainingSessions(remainingSessions));
        } else if (user && user.roleNames.includes("Member")) {
            $('.calendar-remaining-sessions').html(0);
        } else {
            $('.calendar-remaining-sessions-area').hide();
        }
    }

    this.renderSchedule = async function () {
        $('.spinner').show();
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
                        tdEvents.addClass('calendar-today');
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

        adaptUI();
        $('.spinner').hide();
    }

    function formatRemainingSessions(remainingSessions) {
        return remainingSessions < 10 && remainingSessions !== 0 ? "0" + remainingSessions : remainingSessions;
    }

    function initWeek(currentDate) {
        currentDate = moment(currentDate || new Date());
        _self.selectedDay = currentDate;

        var weekStart = currentDate.clone().startOf('isoWeek');

        for (var i = 0; i <= 6; i++) {
            _self.currentDaysOfWeek.push(moment(weekStart).add(i, 'days'));
        }

        _self.selectedDayIndex = _self.currentDaysOfWeek.findIndex(d => d.date() === _self.selectedDay.date());
    }

    async function renderCalendar() {
        // render days of current week header
        renderDaysOfWeek();
        // render schedule data
        await _self.renderSchedule();
    }

    function renderDaysOfWeek() {
        var currentDaysOfWeek = _self.currentDaysOfWeek;
        $('.calendar-control-current-week').html(
            `${currentDaysOfWeek[0].locale('vi').format('DD/MM')} - ${currentDaysOfWeek[6].locale('vi').format('DD/MM')}`
        );
        $('#calendarHead').empty();

        let $tr = $('<tr>').append($('<th>'));
        // current week for desktop / week view
        currentDaysOfWeek
            .forEach(day => {
                let dayLocale = Utils.capitalizeFirstLetter(day.locale('vi').format('dddd D/M'));
                let $th = $('<th>', { class: 'calendar-head' }).text(dayLocale);
                if (day.isSame(new Date(), 'day')) {
                    $th.addClass('calendar-today');
                }
                $th.appendTo($tr);
            });
        $('#calendarHead').append($tr);

        // current week for mobile / day view
        $('.calendar-control-current-week-detail span').each(function (i, e) {
            var date = currentDaysOfWeek[i].date();
            $(e).html(date);
        })
    }

    function renderEventTag(event, isPast) {
        const { schedule } = event;
        const isMember = UserService.isMember();
        const isAdmin = UserService.isAdmin();
        const isReceptionist = UserService.isReceptionist();

        var div = $('<div></div>', {
            'class': 'mistake-event mistake-event-' + schedule.branch.toLowerCase(),
            'data-toggle': 'modal',
            'data-target': (isAdmin || isReceptionist) ? '#modal-manage' : isMember ? '#modal-register' : '',
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

        if ((isAdmin || isReceptionist)) {
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
                div.addClass('mistake-event-past');
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

    function adaptUI() {
        var width = window.screen.width;
        if (width > 1024 && _self.singleDayMode === false) {
            // TODO
        } else {
            _self.singleDayMode = true;
            toggleDaySelector();
            showHideDayByDayIndex(_self.selectedDayIndex);
        }
    }

    // render based on 'prev' or 'next'
    async function renderPrevNextWeek(key) {
        if (key === 'prev') {
            _self.currentDaysOfWeek = _self.currentDaysOfWeek.map(day => day.subtract(7, 'days'));
        } else if (key === 'next') {
            _self.currentDaysOfWeek = _self.currentDaysOfWeek.map(day => day.add(7, 'days'));
        }
        _self.selectedDay = _self.currentDaysOfWeek[_self.selectedDayIndex];
        await renderCalendar();
    }

    function registerEvent() {
        $('.btn-calendar-navigate').click(async function (e) {
            var key = e.currentTarget.id;
            var { currentDaysOfWeek, selectedDay, selectedDayIndex } = _self;
            if (_self.singleDayMode && selectedDay) {
                if (selectedDayIndex === 0 && key === 'prev') {
                    _self.selectedDayIndex = currentDaysOfWeek.length - 1;
                    await renderPrevNextWeek(key);
                } else if (selectedDayIndex == currentDaysOfWeek.length - 1 && key === 'next') {
                    _self.selectedDayIndex = 0;
                    await renderPrevNextWeek(key);
                } else {
                    _self.selectedDayIndex = _self.selectedDayIndex + (key === 'prev' ? -1 : 1);
                    _self.selectedDay = currentDaysOfWeek[_self.selectedDayIndex];
                    toggleDaySelector();
                }
            } else {
                _self.selectedDayIndex = key === 'prev' ? currentDaysOfWeek.length - 1 : 0;
                renderPrevNextWeek(e.currentTarget.id);
            }
        });

        $('.btn-switch-view').on('click', function (e) {
            if (_self.singleDayMode) {
                $('#calendarHead tr').children().show().not(':first-child').addClass('calendar-head');
                $('#calendarBody tr').show().children().show();
                $('.btn-day-of-week').eq(_self.selectedDayIndex).closest('.btn').removeClass('active');
                $('.btn-day-of-week').eq(_self.selectedDayIndex).prop('checked', false);
                _self.singleDayMode = false;
            } else {
                toggleDaySelector();
            }
            toggleSwitchViewLabel();
        });

        $('.btn-day-of-week').on('change', function (e) {
            if (!e.currentTarget.checked) {
                return;
            }
            var index = $(e.currentTarget).data('index');
            _self.selectedDay = _self.currentDaysOfWeek[index];
            _self.selectedDayIndex = index;
            _self.singleDayMode = true;
            showHideDayByDayIndex(index);
            toggleSwitchViewLabel();
        });
    }

    function toggleDaySelector() {
        $('.btn-day-of-week').eq(_self.selectedDayIndex).closest('.btn').button('toggle');
    }

    function toggleSwitchViewLabel() {
        $('.btn-switch-view').html(_self.singleDayMode ? 'Lịch tuần' : 'Lịch ngày')
    }

    // show hide by index in current day of weeks
    function showHideDayByDayIndex(index) {
        $('#calendar td, #calendar th').not(':first-child').hide();
        $('#calendarHead tr th').removeClass('calendar-head');

        index += 2; // nth-child start from one, excluding the first row that indicates time
        $('#calendar th:nth-child(' + index + ')').show();

        var $shownTds = $('#calendar td:nth-child(' + index + ')');
        $shownTds.show();

        if (!UserService.isAdmin()) {
            $('#calendarBody tr').hide();
            $shownTds.find('.mistake-event').closest('tr').show();
        }
    }

    function registerDeviceEvent() {
        if (window.screen.width > 1024) {
            return;
        }

        $('#calendar').swipe({
            swipeLeft: function (event, direction, distance, duration, fingerCount, fingerData) {
                $('button#next').trigger('click');
            },
            swipeRight: function (event, direction, distance, duration, fingerCount, fingerData) {
                $('button#prev').trigger('click');
            },
        })
    }
}
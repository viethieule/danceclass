const select2config = {
    tags: true,
    createTag: function (params) {
        return {
            id: params.term,
            text: params.term,
            newOption: true
        }
    },
    templateResult: function (data) {
        var $result = $("<span></span>");

        $result.text(data.text);

        if (data.newOption) {
            $result.append(" <em>(thêm mới)</em>");
        }

        return $result;
    }
}

function ScheduleCreate() {
    var _self = this;
    var _editMode = false;

    this.initScheduleCreate = function () {
        if (!UserService.isAdmin()) {
            return;
        }

        initClass();
        initTrainer();
        initDateTimePicker();
        initCreateScheduleBtn();

        registerEvent();
    }

    this.openScheduleCreateModal = function (isEdit, $triggerElement) {
        _editMode = !!isEdit;
        $('#modal-create-schedule').modal('toggle', $triggerElement);
    }

    async function initClass() {
        try {
            const classes = await ApiService.get('api/class/getAll');
            if (classes) {
                let select = $('#class');
                select.empty();
                classes.forEach((cls, i, a) => {
                    let attr = { value: cls.id };
                    if (i === 0) { attr.selected = 'selected' }
                    $('<option>', attr).text(cls.name).appendTo(select);
                });

                select.select2(select2config);
            }
        } catch (ex) {
            console.log(ex);
        }
    }

    function initTrainer() {
        let select = $('#trainer');
        select.select2(select2config);
    }

    function initDateTimePicker() {
        $('.mistake-datepicker')
            .datepicker({
                autoclose: true,
                format: 'dd/mm/yyyy',
            })
            .inputmask('dd/mm/yyyy', {
                'placeholder': 'dd/mm/yyyy'
            });

        $('.mistake-timepicker').timepicker();
    }

    function initCreateScheduleBtn() {
        var element = $('<div>', { class: 'calendar-create-schedule' })
            .append(
                $('<button>', {
                    'class': 'btn btn-success btn-create-schedule',
                    'data-toggle': 'modal',
                    'data-target': '#modal-create-schedule'
                })
                    .append($('<i>', { class: 'fa fa-plus' }))
                    .append($('<span>').html('&nbsp;&nbsp;Tạo lịch học'))
            );
        element.appendTo('.calendar-control, .header-calendar-control');
    }

    function registerEvent() {
        registerModalEvent();
        registerFormEvent();
        registerDaysPerWeekEvent();
    }

    function registerModalEvent() {
        $('#modal-create-schedule').on('shown.bs.modal', function (event) {
            let $target = $(event.relatedTarget);
            if ($target.is('td')) {
                // From calendar blank cells
                const prevSelectedDate = $(this).data('prevDate');
                const date = $target.data('date');
                if (!prevSelectedDate) {
                    $(this).data('prevDate', date);
                } else if (!date.isSame(prevSelectedDate)) {
                    $(this).data('prevDate', date);
                    FormUtils.reset($('form#create-schedule'), ['#class', '#trainer', '#branch']);
                    resetDaysPerWeekCheckboxes();
                }

                $('#openingDate').datepicker('setDate', date.toDate());
                $('#startTime').timepicker('setTime', date.locale('en').format('h:mm A'));
                $('.days-per-week input:checkbox[value="' + date.day() + '"]').prop('checked', true);
            } else if (_editMode && _self.selectedSchedule && $target.hasClass('btn-schedule-update')) {
                // From edit button in manage schedule modal (also check not the create schedule button above calendar)
                console.log(_editMode);
                console.log(_self.selectedSchedule);
                console.log(_self.selectedScheduleDetails);

                var { selectedSchedule, selectedScheduleDetails } = _self;
                var { classId, song, openingDate, startTime, daysPerWeek, sessions, trainerId, branch } = selectedSchedule;

                daysPerWeek = daysPerWeek.split('');
                $('#class').val(classId);
                $('#song').val(song);
                $('#openingDate').datepicker('setDate', new Date(openingDate));
                $('#startTime').timepicker('setTime', startTime);
                $('.days-per-week input:checkbox').each(function (i, e) {
                    $(this).prop('checked', false);
                    if (daysPerWeek.indexOf($(this).val()) !== -1) {
                        $(this).prop('checked', true);
                    };
                });

                $('#sessions').val(sessions);
                $('#trainer').val(trainerId);
                $('#branch').val(branch);
            }

            if (_editMode) {
                $('btn-action').html('Sửa');
            } else {
                $('btn-action').html('Tạo');
            }
        });

        $('#modal-create-schedule').on('hide.bs.modal', function (e) {
            _editMode = false;
        });
    }

    function registerFormEvent() {
        $('form#create-schedule')
            .submit(function (event) {
                event.preventDefault();
            })
            .validate(Object.assign(jqueryValidateConfig, {
                rules: {
                    class: {
                        required: true
                    },
                    openingDate: {
                        required: true
                    },
                    startTime: {
                        required: true
                    },
                    sessions: {
                        required: {
                            depends: function (element) {
                                return $('.days-per-week input:checkbox:checked').length !== 0;
                            }
                        }
                    }
                },
                submitHandler: async function (form) {
                    $('#modal-create-schedule .modal-body').alert(false);
                    $('.btn-exit, .btn-action').prop('disabled', true);
                    let $form = $(form);
                    let { song, sessions, branch, class: cls, trainer, startTime } = FormUtils.convertFormDataToDictionary($form.serializeArray());

                    let daysPerWeek = $('.days-per-week input:checkbox:checked')
                        .map(function () { return this.value }).get().sort().join('');

                    let openingDate = $('#openingDate').datepicker('getDate');
                    openingDate = moment(openingDate).format('MM-DD-YYYY');

                    if (startTime) {
                        let [time, meridiem] = startTime.trim().split(' ');
                        let [hour, minute] = time.split(':');
                        if (meridiem === 'PM' && hour !== '12') {
                            hour = parseInt(hour) + 12;
                        }
                        startTime = hour + ':' + minute;
                    }

                    let schedule = { song, openingDate, startTime, sessions, daysPerWeek, branch }

                    let isRerenderClass = false;
                    let isRerenderTrainer = false;

                    if (cls && isNaN(cls)) {
                        schedule.className = cls;
                        isRerenderClass = true;
                    } else if (cls) {
                        schedule.classId = parseInt(cls)
                    }

                    if (trainer && isNaN(trainer)) {
                        schedule.trainerName = trainer;
                        isRerenderTrainer = true;
                    } else if (trainer) {
                        schedule.trainerId = parseInt(trainer)
                    }

                    try {
                        if (_editMode) {
                            schedule.id = _self.selectedSchedule.id;
                            var rs = await ApiService.post('api/schedule/update', { schedule, selectedScheduleDetailId: _self.selectedScheduleDetails.id });
                            _editMode = false;
                            $('#modal-create-schedule').modal('hide');

                            await _self.renderSchedule();
                            if (rs && rs.IsSelectedSessionDeleted) {
                                _self.selectedScheduleDetails = null;
                                $('#modal-manage .modal-body').alert(true, 'Lịch học đã bị xóa sau khi cập nhật!');
                                setTimeout(function () {
                                    $('#modal-manage').modal('hide');
                                }, 2000)
                            } else {
                                var message = rs && rs.messages ? ('<ul>' + rs.messages.map(function (m) { return '<li>' + m + '</li>' }).join('') + '</ul>') : ''
                                _self.reloadManageModal(message);
                            }
                        } else {
                            await ApiService.post('api/schedule/create', { schedule });
                            $('#modal-create-schedule').modal('hide');
                            _self.renderSchedule();
                        }
                        if (isRerenderClass) {
                            initClass();
                        }
                        if (isRerenderTrainer) {
                            // TODO: initTrainer()
                        }
                        resetCreateForm();
                    } catch (ex) {
                        console.log(ex);
                        $('#modal-create-schedule .modal-body').alert(true, ex);
                    } finally {
                        $('.btn-exit, .btn-action').prop('disabled', false);
                    }
                }
            }))
    }

    function resetCreateForm() {
        FormUtils.reset($('form#create-schedule'), ['#class', '#trainer', '#branch']);
        $('#openingDate').datepicker('setDate', null);
        $('#startTime').timepicker('setTime', null);

        resetDaysPerWeekCheckboxes();
    }

    function resetDaysPerWeekCheckboxes() {
        $('.days-per-week input:checkbox').off('change').prop('checked', false).change(daysPerWeekChangeHandler);
    }

    function registerDaysPerWeekEvent() {
        $('.days-per-week input:checkbox').change(daysPerWeekChangeHandler)
    }

    function daysPerWeekChangeHandler() {
        let $label = $(`label[for="sessions"]`)
        if ($('.days-per-week input:checkbox:checked').length === 0) {
            $label.removeClass('required');
        } else {
            $label.addClass('required');
        }
    }
}
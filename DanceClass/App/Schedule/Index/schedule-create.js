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
                    'class': 'btn btn-success',
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
            if (!$target.is(':button')) {
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
            }
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
                        await ApiService.post('api/schedule/create', { schedule });
                        $('#modal-create-schedule').modal('hide');
                        _self.renderSchedule();
                        if (isRerenderClass) {
                            initClass();
                        }
                        resetCreateForm();
                    } catch (ex) {
                        console.log(ex);
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
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

function initCreateScheduleForm() {
    if (!userService.isAdmin()) {
        return;
    }

    initClass();

    initTrainer();

    initDateTimePicker();

    initCreateScheduleBtn();
}

async function initClass() {
    try {
        const classes = await apiService.get('api/class/getAll');
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

function registerCreateScheduleModal() {
    if (!userService.isAdmin()) {
        return;
    }

    registerModalEvent();
    registerFormEvent();
    registerDaysPerWeekEvent();
}

function initCreateScheduleBtn() {
    $('<div>', { class: 'calendar-create-schedule' })
        .append(
            $('<button>', {
                'class': 'btn btn-success',
                'data-toggle': 'modal',
                'data-target': '#modal-create-schedule'
            })
                .text('Tạo lịch học')
        )
        .appendTo($('.calendar-control'));
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
                resetForm($('form#create-schedule'), ['#class', '#trainer', '#branch']);
            }

            $('#openingDate').datepicker('setDate', date.toDate());
            $('#startTime').timepicker('setTime', date.locale('en').format('h:mm A'));
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
                debugger;
                let $form = $(form);
                let { song, sessions, branch, class: cls, trainer, startTime } = convertFormDataToDictionary($form.serializeArray());

                let daysPerWeek = $('.days-per-week input:checkbox:checked')
                    .map(function () { return this.value }).get().sort().join('');

                let openingDate = $('#openingDate').datepicker('getDate');
                openingDate = moment(openingDate).format('MM-DD-YYYY');

                if (startTime) {
                    let [time, meridiem] = startTime.trim().split(' ');
                    let [hour, minute] = time.split(':');
                    if (meridiem === 'PM') {
                        if (hour !== '12') {
                            hour = parseInt(hour) + 12;
                            startTime = hour + ':' + minute;
                        }
                    } else {
                        startTime = hour + ':' + minute;
                    }
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
                    await apiService.post('api/schedule/create', { schedule });
                    $('#modal-create-schedule').modal('hide');
                    renderSchedule();
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
    resetForm($('form#create-schedule'), ['#class', '#trainer', '#branch']);
    $('#openingDate').datepicker('setDate', null);
    $('#startTime').timepicker('setTime', null);
}

function registerDaysPerWeekEvent() {
    $('.days-per-week input:checkbox').change(function () {
        let $label =  $(`label[for="sessions"]`)
        if ($('.days-per-week input:checkbox:checked').length === 0) {
            $label.removeClass('required');
        } else {
            $label.addClass('required');
        }
    })
}
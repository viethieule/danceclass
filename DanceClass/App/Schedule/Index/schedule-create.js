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
    var _deleteCreateMode = false;

    this.initScheduleCreate = function () {
        if (!UserService.isAdmin() && !UserService.isCollaborator()) {
            return;
        }

        initClass();
        initTrainer();
        initDateTimePicker();
        initCreateScheduleBtn();

        registerEvent();
    }

    this.openScheduleCreateModal = function (mode, $triggerElement) {
        if (mode === 'edit') {
            _editMode = true;
        } else if (mode === 'deleteCreate') {
            _deleteCreateMode = true;
        }
        $('#modal-create-schedule').modal('toggle', $triggerElement);
    }

    async function initClass() {
        try {
            const classes = await ApiService.get('api/class/getAll');
            $('#class').appendOptions(classes, 'id', 'name').select2(select2config);
        } catch (ex) {
            console.log(ex);
        }
    }

    async function initTrainer() {
        try {
            const trainers = await ApiService.get('api/trainer/getAll');
            $('#trainer').appendOptions(trainers, 'id', 'name').select2(select2config);
        } catch (ex) {
            console.log(ex);
        }
    }

    function initDateTimePicker() {
        $('.mistake-datepicker')
            .datepicker({
                autoclose: true,
                format: 'dd/mm/yyyy',
            })
            .inputmask('dd/mm/yyyy', {
                'placeholder': 'dd/mm/yyyy'
            })
            .on('hide', function (e) {
                e.stopPropagation();
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
                    .append($('<span>', { class: 'hidden-xs' }).html('&nbsp;&nbsp;Tạo lịch học'))
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
            $('#modal-create-schedule .modal-body').alert(false);
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
                var { selectedSchedule, selectedScheduleDetails } = _self;
                var { classId, song, openingDate, startTime, daysPerWeek, sessions, trainerId, branch } = selectedSchedule;

                daysPerWeek = daysPerWeek.split('');
                $('#class').val(classId).trigger('change');
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
                $('#trainer').val(trainerId).trigger('change');
                $('#branch').val(branch);
            } else if (_deleteCreateMode && _self.selectedScheduleDetails && $target.hasClass('btn-schedule-delete-create')) {
                var { selectedSchedule, selectedScheduleDetails } = _self;
                var { classId, song, startTime, daysPerWeek, sessions, trainerId, branch } = selectedSchedule;
                var { date } = selectedScheduleDetails;

                daysPerWeek = daysPerWeek.split('');
                $('#class').val(classId).trigger('change');
                $('#song').val('');
                $('#openingDate').datepicker('setDate', new Date(date));
                $('#startTime').timepicker('setTime', startTime);
                $('.days-per-week input:checkbox').each(function (i, e) {
                    $(this).prop('checked', false);
                    if (daysPerWeek.indexOf($(this).val()) !== -1) {
                        $(this).prop('checked', true);
                    };
                });

                $('#sessions').val('');
                $('#trainer').val(trainerId);
                $('#branch').val(branch);
            }

            if (_editMode) {
                $(this).find('#btn-action').html('Sửa');
                $(this).find('.modal-title').html('Sửa lịch học');
            } else if (_deleteCreateMode) {
                $(this).find('#btn-action').html('Xóa và tạo mới');
                $(this).find('.modal-title').html('Xóa và tạo mới lịch học');
            } else {
                $(this).find('#btn-action').html('Tạo');
                $(this).find('.modal-title').html('Tạo lịch học');
            }
        });

        $('#modal-create-schedule').on('hide.bs.modal', function (e) {
            _editMode = false;
            _deleteCreateMode = false;
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

                    let { song, sessions, branch, class: cls, trainer, startTime, isPrivate } = FormUtils.convertFormDataToDictionary($form.serializeArray());

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

                    let schedule = { song, openingDate, startTime, sessions, daysPerWeek, branch, isPrivate: isPrivate === 'true' }

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
                            if (rs) {
                                var message = rs.messages && rs.messages.length > 0 ? ('<ul>' + rs.messages.map(function (m) { return '<li>' + m + '</li>' }).join('') + '</ul>') : '';
                                if (rs.isSelectedSessionDeleted) {
                                    _self.selectedScheduleDetails = null;
                                    $('#modal-manage .modal-body').alert(true, 'warning', message);
                                } else if (rs.isSelectedSessionUpdated) {
                                    _self.reloadManageModal(rs.updatedSessionId, 'warning', message);
                                } else {
                                    _self.reloadManageModal(_self.selectedScheduleDetails.id, 'warning', message);
                                }
                                $('#modal-manage .modal-body').alert(true, 'success', 'Sửa thành công', 2500);
                            }
                        } else if (_deleteCreateMode) {
                            var message = 'Bạn có chắc chắn muốn xóa và tạo mới lịch học?';
                            _self.showAlert(message, function (event) {
                                var deleteAjax = ApiService.del('/api/schedule/deleteSession/' + _self.selectedScheduleDetails.id);
                                var createAjax = ApiService.post('api/schedule/create', { schedule });
                                $.when(deleteAjax, createAjax).then(function (deleteRs, createRs) {
                                    if (createRs && deleteRs && createRs[0] && deleteRs[0] && deleteRs[0].success) {
                                        $('#modal-create-schedule .modal-body').alert(true, 'success', 'Xóa và tạo mới lịch học thành công');
                                        _self.selectedScheduleDetails = null;
                                        _self.selectedSchedule = null;
                                        setTimeout(function () {
                                            $('#modal-manage').modal('hide');
                                            $('#modal-create-schedule').modal('hide');
                                        }, 2000);
                                    } else {
                                        $('#modal-create-schedule .modal-body').alert(true, 'danger', 'Xóa hoặc tạo mới không thành công');
                                    }
                                }, function (jqXHR, textStatus, errorThrown) {
                                    console.log(errorThrown);
                                    $('#modal-create-schedule .modal-body').alert(true, 'danger', errorThrown);
                                });

                                postFormSubmit(isRerenderClass, isRerenderTrainer);
                            });
                            return;
                        } else {
                            await ApiService.post('api/schedule/create', { schedule });
                            _self.renderSchedule();
                            $('#modal-create-schedule').modal('hide');
                        }

                        postFormSubmit(isRerenderClass, isRerenderTrainer);
                    } catch (ex) {
                        console.log(ex);
                        $('#modal-create-schedule .modal-body').alert(true, 'danger', ex);
                    } finally {
                        $('.btn-exit, .btn-action').prop('disabled', false);
                    }
                }
            }))
    }

    function postFormSubmit(isRerenderClass, isRerenderTrainer) {
        if (isRerenderClass) {
            initClass();
        }

        if (isRerenderTrainer) {
            initTrainer()
        }

        resetCreateForm();
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
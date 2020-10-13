function EditPackageController() {
    var _self = this;
    var _defaultPackages = [];

    this.initEditPackage = function () {
        initExpiryDatePicker();
        adjustInterfaceBasedOnRole();
        registerEvent();
    }

    this.loadMembershipData = function () {
        var { member } = _self;
        if (member && member.membership) {
            var { membership } = member;

            // info tab
            $('#remainingSessions').text(membership.remainingSessions);
            $('#expiryDate').text(moment(membership.expiryDate).format('DD/MM/YYYY'));

            // modal
            $('#membership-expiry-date').datepicker('setDate', new Date(membership.expiryDate));
            $('#membership-remaining-sessions').val(membership.remainingSessions);
        }
    }

    this.loadMemberPackages = async function () {
        _defaultPackages = (_defaultPackages && _defaultPackages.length > 0) ? _defaultPackages : await ApiService.get('/api/package/getDefaults');
        var isAdmin = UserService.isAdmin();

        var packageCellRenderer = function (data, type, row, classSuffix, template) {
            if (type !== 'display') {
                return data;
            }

            var html = data;
            if (isAdmin) {
                var input = $('<input>', { type: 'text', class: 'form-control list-package-cell list-package-' + classSuffix, value: data });
                if (row.defaultPackageId) {
                    input.prop('disabled', true);
                }
                html = input.wrap('<div></div>').parent().html();
            }

            if (template) {
                return template.replace(/_data_/g, html);
            }
            return html;
        }

        if (!$.fn.DataTable.isDataTable('#list-packages')) {
            $('#list-packages').dataTable({
                ajax: {
                    url: '/api/package/getByUserId',
                    type: 'POST',
                    data: { userId: _self.member.id },
                    dataSrc: 'packages',
                    error: function (xhr, textStatus, error) { console.log(textStatus); }
                },
                columns: [
                    { title: '', data: 'id', visible: false },
                    {
                        title: 'Gói tập',
                        data: 'defaultPackageId',
                        render: function (data, type, row, meta) {
                            if (type !== 'display') {
                                return data;
                            }

                            var select = $('<select>', { class: 'form-control list-package-default-package' });
                            if (data) {
                                select.appendOptions(_defaultPackages, 'id', 'numberOfSessions', data);
                                $('<option>', { value: -1 })
                                    .text('Khác...')
                                    .appendTo(select);
                            }
                            else {
                                select.appendOptions(_defaultPackages, 'id', 'numberOfSessions', -1);
                                $('<option>', { value: -1, selected: 'selected' })
                                    .text('Khác...')
                                    .appendTo(select);
                            }

                            return select.wrap('<div></div>').parent().html();
                        },
                        visible: isAdmin,
                        width: '10%'
                    },
                    {
                        title: 'Hạn',
                        data: 'months',
                        render: function (data, type, row, meta) {
                            return packageCellRenderer(data, type, row, 'months', '_data_ tháng');
                        },
                        width: '10%'
                    },
                    {
                        title: 'Số buổi',
                        data: 'numberOfSessions',
                        render: function (data, type, row, meta) {
                            return packageCellRenderer(data, type, row, 'numberOfSessions');
                        },
                        width: '5%'
                    },
                    {
                        title: 'Giá',
                        data: 'price',
                        render: function (data, type, row, meta) {
                            return packageCellRenderer(data, type, row, 'price');
                        },
                        width: '5%'
                    },
                    { title: 'Số buổi còn lại', data: 'remainingSessions', width: '12%' },
                    { title: 'Trạng thái', data: 'status', width: '10%' },
                    {
                        title: '',
                        data: 'id',
                        render: function (data, type, row, meta) {
                            //var btnEdit = $('<button>', { class: 'btn btn-xs btn-package btn-success btn-package-edit', 'data-package-id': data }).append($('<i>', { class: 'fa fa-pencil' }));
                            var btnSave = $('<button>', { class: 'btn btn-xs btn-package btn-success btn-package-save', 'data-package-id': data }).append($('<i>', { class: 'fa fa-check' }));
                            var btnCancel = $('<button>', { class: 'btn btn-xs btn-package btn-danger btn-package-cancel', 'data-package-id': data }).append($('<i>', { class: 'fa fa-times' }));
                            var div = $('<div>', { class: 'list-package-actions', style: 'display: none' }).append(btnSave).append(btnCancel);
                            return div.wrap('<div></div>').parent().html();
                        },
                        visible: false,
                        width: '10%'
                    }
                ],
                ordering: false,
                pageLength: 3,
                lengthChange: false,
                searching: false,
                language: {
                    lengthMenu: 'Hiển thị _MENU_ gói tập trên trang',
                    info: 'Trang _PAGE_ trên _PAGES_',
                    zeroRecords: 'Chưa đăng ký gói tập nào',
                    infoEmpty: '',
                    search: 'Tìm kiếm',
                    paginate: {
                        previous: 'Trước',
                        next: 'Sau'
                    }
                }
            });
        } else {
            $('#list-packages').DataTable().ajax.reload();
        }
    }

    function initExpiryDatePicker() {
        $('#membership-expiry-date')
            .datepicker({
                autoclose: true,
                format: 'dd/mm/yyyy',
            })
            .inputmask('dd/mm/yyyy', {
                'placeholder': 'dd/mm/yyyy'
            });
    }

    function adjustInterfaceBasedOnRole() {
        if (!UserService.isAdmin()) {
            $('.membership-container').remove();
            $('#editPackage').removeClass('btn-primary').addClass('btn-link').html('Xem tất cả gói tập')
        }
    }

    function registerEvent() {
        if (UserService.isAdmin()) {
            registerAdminEvent();
        }
    }

    function registerAdminEvent() {
        $('.btn-edit').on('click', function (event) {
            var formGroup = $(this).closest('.form-group');
            var input = formGroup.find('input');
            input.focus();
        });

        $('.btn-cancel-edit').on('click', function (event) {
            var formGroup = $(this).closest('.form-group');
            formGroup.find('.btn-save-edit, .btn-cancel-edit').closest('div').hide();
            formGroup.find('.btn-edit').closest('div').show();

            var input = formGroup.find('input');
            input.focusout();

            var propName = input.prop('name');
            var { membership } = _self.member;
            if (input.hasClass('form-edit-datepicker')) {
                input.datepicker('setDate', new Date(membership[propName]));
            } else {
                input.val(membership[propName]);
            }
        });

        $('.btn-save-edit').on('click', async function (event) {
            var formGroup = $(this).closest('.form-group');

            var input = formGroup.find('input');

            var propName = input.prop('name');
            var { membership } = _self.member;
            var value = input.val();
            var unchanged = value == membership[propName];

            if (input.hasClass('form-edit-datepicker')) {
                var datepickerVal = input.datepicker('getDate');
                value = moment(datepickerVal).format('MM-DD-YYYY');
                unchanged = moment(datepickerVal).isSame(moment(membership[propName]), 'day');
            }

            if (!unchanged) {
                $(this).loading(true);
                formGroup.find('.btn-cancel-edit').prop('disabled', true)
                input.prop('disabled', true);

                try {
                    var rs = await ApiService.post('/api/membership/update', {
                        userId: _self.member.id,
                        [propName]: value
                    });
                    if (rs && rs.membership) {
                        _self.member.membership = rs.membership;
                        _self.loadMembershipData();
                    }
                } catch (ex) {
                    $('#modal-edit-package .modal-body').alert(true, 'danger', ex);
                    _self.loadMembershipData();
                } finally {
                    $(this).loading(false);
                    input.prop('disabled', false);
                    formGroup.find('.btn-save-edit, .btn-cancel-edit').prop('disabled', false).closest('div').hide();
                    formGroup.find('.btn-edit').closest('div').show();
                }
            } else {
                formGroup.find('.btn-save-edit, .btn-cancel-edit').prop('disabled', false).closest('div').hide();
                formGroup.find('.btn-edit').closest('div').show();
            }
        })

        $('.form-edit').on('focus', function (event) {
            var formGroup = $(this).closest('.form-group');
            formGroup.find('.btn-edit').closest('div').hide();
            formGroup.find('.btn-cancel-edit, .btn-save-edit').closest('div').show();
        });

        registerPackagesTableEvent();
    }

    function registerPackagesTableEvent() {
        $('#list-packages').on('change', '.list-package-default-package', function (event) {
            var table = $('#list-packages').DataTable();
            var row = $(this).closest('tr');

            removeRowValidation(row);

            var package = table.row(row).data();
            var dbDefaultPackageId = package.defaultPackageId;

            var actionsColumn = table.column(7);
            if (dbDefaultPackageId && dbDefaultPackageId === parseInt(this.value)) {
                toggleActionButtons(actionsColumn, row, false);
            } else {
                toggleActionButtons(actionsColumn, row, true);
            }

            if (this.value == -1) {
                row.find('.list-package-months, .list-package-numberOfSessions, .list-package-price').val('').prop('disabled', false);
                row.find('.list-package-months').focus();
            } else {
                var selectedDefaultPkg = _defaultPackages.find(p => p.id == this.value);
                if (selectedDefaultPkg) {
                    row.find('.list-package-months, .list-package-numberOfSessions, .list-package-price').prop('disabled', true);
                    row.find('.list-package-months').val(selectedDefaultPkg.months);
                    row.find('.list-package-numberOfSessions').val(selectedDefaultPkg.numberOfSessions);
                    row.find('.list-package-price').val(selectedDefaultPkg.price);
                }
            }
        });

        $('#list-packages').on(
            'input',
            '.list-package-months, .list-package-numberOfSessions, .list-package-price',
            function (event) {
                $(this).removeClass('list-package-has-error');

                var row = $(this).closest('tr')
                if (row.find('.list-package-default-package').val() != -1) {
                    return;
                }

                var table = $('#list-packages').DataTable();
                toggleActionButtons(table.column(7), row, true);
            }
        )

        $('#list-packages').on('click', '.btn-package-save', async function (event) {
            var table = $('#list-packages').DataTable();
            var row = $(this).closest('tr');
            var packageRow = table.row(row);
            var package = packageRow.data();

            // Validate
            var updatedDefaultPackageId = parseInt(row.find('.list-package-default-package').val());
            var updatedNumberOfSessions = row.find('.list-package-numberOfSessions').val();
            var updatedPrice = row.find('.list-package-price').val();
            var updatedMonths = row.find('.list-package-months').val();

            var isValidate = true;
            row.find('input').each(function (i, e) {
                if (!this.value && this.value !== 0) {
                    isValidate = false;
                    $(this).addClass('list-package-has-error');
                }
            });

            if (!isValidate) {
                $('#modal-edit-package .modal-body').alert(true, 'warning', 'Vui lòng nhập đầy đủ thông tin Hạn, Số buổi và Giá', 5000);
                return;
            }

            var data = null;
            if (package.defaultPackageId) {
                // update from a default pck to different pkg
                if (updatedDefaultPackageId === -1) {
                    data = {
                        numberOfSessions: updatedNumberOfSessions,
                        price: updatedPrice,
                        months: updatedMonths,
                    }
                // update from a default pck to a different default pkg
                } else if (updatedDefaultPackageId !== package.defaultPackageId) {
                    data = { defaultPackageId: updatedDefaultPackageId }
                }
            } else {
                // update from not a default pck to a default pkg
                if (updatedDefaultPackageId !== -1) {
                    data = { defaultPackageId: updatedDefaultPackageId }
                // update from not a default pck to not a default pkg but maybe different info
                } else if (package.numberOfSessions != updatedNumberOfSessions || package.price != updatedPrice || package.months != updatedMonths) {
                    data = {
                        numberOfSessions: updatedNumberOfSessions,
                        price: updatedPrice,
                        months: updatedMonths,
                    }
                }
            }

            if (data) {
                try {
                    var btnCancelEdit = row.find('.btn-cancel-edit');
                    $(this).loading(true);
                    data.packageId = package.id;
                    var rs = await ApiService.post('/api/package/edit', data);
                    if (rs && rs.membership && rs.package) {
                        $('#modal-edit-package .modal-body').alert(true, 'success', 'Sửa thành công. Vui lòng kiểm tra lại số buổi còn lại và ngày hết hạn', 5000);
                        _self.member.membership = rs.membership;
                        _self.loadMembershipData();
                        packageRow.data(rs.package);
                    } else {
                        $('#modal-edit-package .modal-body').alert(true, 'danger', 'Sửa không thành công');
                        handleCancelEdit(btnCancelEdit);
                    }
                } catch (ex) {
                    $('#modal-edit-package .modal-body').alert(true, 'danger', ex);
                    handleCancelEdit(btnCancelEdit);
                } finally {
                    $(this).loading(false);
                }
            }

            toggleActionButtons(table.column(7), row, false);
        });

        $('#list-packages').on('click', '.btn-package-cancel', function (event) {
            handleCancelEdit($(this));
        });

        function removeRowValidation(row) {
            row.find('input').each(function (i, e) {
                $(this).removeClass('list-package-has-error');
            });
        }

        function toggleActionButtons(columns, tr, isShow) {
            if (isShow) {
                columns.visible(isShow);
                tr.find('.list-package-actions').toggle(isShow);
            } else {
                tr.find('.list-package-actions').toggle(isShow);
                var otherTr = tr.closest('tbody').find('.list-package-actions:visible').not(tr);
                if (otherTr.length === 0) {
                    columns.visible(isShow);
                }
            }
        }

        function handleCancelEdit(btnCancelEdit) {
            var table = $('#list-packages').DataTable();
            var row = btnCancelEdit.closest('tr');

            removeRowValidation(row);

            var package = table.row(row).data();
            if (package) {
                var isDefaultPackage = !!package.defaultPackageId
                row.find('.list-package-default-package').val(package.defaultPackageId || -1);
                row.find('.list-package-months').val(package.months).prop('disabled', isDefaultPackage);
                row.find('.list-package-numberOfSessions').val(package.numberOfSessions).prop('disabled', isDefaultPackage);
                row.find('.list-package-price').val(package.price).prop('disabled', isDefaultPackage);
            }

            toggleActionButtons(table.column(7), row, false);
        }
    }
}
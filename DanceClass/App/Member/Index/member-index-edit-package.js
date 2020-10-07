function EditPackageController() {
    var _self = this;
    var _defaultPackages = [];

    this.initEditPackage = function () {
        initExpiryDatePicker();
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
        _defaultPackages = await ApiService.get('/api/package/getDefaults');
        var isAdmin = UserService.isAdmin();

        var packageCellRenderer = function (data, type, classSuffix, template) {
            if (type !== 'display') {
                return data;
            }

            var html = data;
            if (isAdmin) {
                html = $('<input>', { type: 'text', class: 'form-control list-package-cell list-package-' + classSuffix, value: data }).wrap('<div></div>').parent().html();
            }

            if (template) {
                return template.replace(/_data_/g, html);
            }
            return html;
        }

        $('.list-packages').dataTable({
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

                        var select = $('<input>', { type: 'select', name: 'package', class: 'form-control list-package-default-package' })
                            .appendOptions(_defaultPackages, 'id', 'name')

                        $('<option>', { value: -1 })
                            .text('Khác...')
                            .appendTo(select);

                        select.find('option[value="' + data + '"]').prop('selected', true);
                        return select.wrap('<div></div>').parent().html();
                    },
                    visible: isAdmin,
                    width: '20%'
                },
                {
                    title: 'Hạn',
                    data: 'months',
                    render: function (data, type, row, meta) {
                        return packageCellRenderer(data, type, 'months', '_data_ tháng');
                    }
                },
                {
                    title: 'Số buổi',
                    data: 'numberOfSessions',
                    render: function (data, type, row, meta) {
                        return packageCellRenderer(data, type, 'numberOfSessions');
                    }
                },
                {
                    title: 'Giá',
                    data: 'price',
                    render: function (data, type, row, meta) {
                        return packageCellRenderer(data, type, 'price');
                    }
                },
                { title: 'Số buổi còn lại', data: 'remainingSessions' },
                { title: '', data: 'status' },
                {
                    title: '',
                    data: 'id',
                    render: function (data, type, row, meta) {
                        var btnEdit = $('<button>', { class: 'btn btn-success btn-package-edit', 'data-package-id': data }).append($('<i>', { class: 'fa fa-pencil' }));
                        var btnSave = $('<button>', { class: 'btn btn-success btn-package-save', 'data-package-id': data }).append($('<i>', { class: 'fa fa-check' }));
                        var btnCancel = $('<button>', { class: 'btn btn-danger btn-package-cancel', 'data-package-id': data }).append($('<i>', { class: 'fa fa-times' }));
                        var div = $('<div>').append(btnEdit).append(btnSave).append(btnCancel);
                        return div.html();
                    },
                    visible: isAdmin
                }
            ],
            pageLength: 5,
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
    }
}
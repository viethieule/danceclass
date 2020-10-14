function CreatePackageController() {
    var _self = this;
    var _packages = [];

    this.initCreatePackage = async function () {
        await initSelectPackages();
        adjustInterfaceBasedOnRole();
        registerEvent();
    }

    async function initSelectPackages() {
        try {
            _packages = await ApiService.get('/api/package/getDefaults');

            var select = $('#package');
            _packages.forEach(package => {
                $('<option></option>', { value: package.id })
                    .text(package.numberOfSessions)
                    .appendTo(select);
            });

            $('<option></option>', { value: -1 })
                .text('Khác...')
                .appendTo(select);

        } catch (err) {
            console.log(err);
        }
    }

    function adjustInterfaceBasedOnRole() {
        if (!UserService.isAdmin()) {
            $('#addPackage').remove();
        }
    }

    function registerEvent() {
        $('form#createPackage')
            .submit(function (event) {
                event.preventDefault();
            })
            .validate(Object.assign(jqueryValidateConfig, {
                rules: {
                    package: {
                        required: true
                    },
                    expired: {
                        required: {
                            depends: function (element) {
                                return $('#package').val() === "-1";
                            }
                        }
                    },
                    sessions: {
                        required: {
                            depends: function (element) {
                                return $('#package').val() === "-1";
                            }
                        }
                    },
                    price: {
                        required: {
                            depends: function (element) {
                                return $('#package').val() === "-1";
                            }
                        }
                    },
                },
                submitHandler: async function (form) {
                    $('#modal-create-package .modal-body').alert(false);
                    $('#btn-create-package').prop('disabled', true);

                    var $form = $(form);
                    var { package: defaultPackageId, sessions, price, expired } = FormUtils.convertFormDataToDictionary($form.serializeArray());

                    var data = {
                        userId: _self.member.id
                    };

                    if (defaultPackageId !== "-1") {
                        data.defaultPackageId = defaultPackageId;
                    } else {
                        data.numberOfSessions = sessions;
                        data.price = price;
                        data.months = expired;
                    }

                    try {
                        var rs = await ApiService.post('/api/package/add', data);
                        if (rs && rs.membership) {
                            _self.member.membership = rs.membership;
                            _self.loadUser();
                        }
                        $('#modal-create-package').modal('hide');
                        $('.content-header').alert(true, 'success', 'Thêm thành công', 5000);
                    } catch (ex) {
                        $('#modal-create-package .modal-body').alert(true, 'danger', ex);
                        console.log(ex);
                    } finally {
                        $('#btn-create-package').prop('disabled', false);
                    }
                }
            }))

        $('select#package')
            .on('change', function (e) {
                manipulateFieldsOnOtherPackage(['expired', 'sessions', 'price'], this.value);
            })
            .trigger('change');
    }

    function manipulateFieldsOnOtherPackage(fieldIds, packageId) {
        var isOtherPackage = packageId === "-1";

        fieldIds.forEach(fieldId => {
            addRemoveRequired(fieldId, isOtherPackage);

            var jElement = $('#' + fieldId);
            jElement.prop('disabled', !isOtherPackage);

            if (isOtherPackage) {
                jElement.val("");
            } else {
                var package = _packages.find(p => p.id.toString() === packageId);
                var prop = fieldId === 'expired' ? "months" : fieldId === 'sessions' ? "numberOfSessions" : fieldId === 'price' ? "price" : "";

                if (prop) {
                    jElement.val(package[prop]);
                    jElement.closest('.form-group').removeClass('has-error');
                }
            }
        });
    }

    function addRemoveRequired(name, isRequired) {
        var ele = $(`label[for="${name}"]`);
        if (isRequired) {
            ele.addClass('required');
        } else {
            ele.removeClass('required');
        }
    }
}
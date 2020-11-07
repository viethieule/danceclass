function CreateReceptionistController() {
    this.receptionist = null;

    this.initialize = function () {
        registerEvent();
    }

    function registerEvent() {
        $('form#createReceptionist')
            .submit(function (event) {
                event.preventDefault();
            })
            .validate(Object.assign(jqueryValidateConfig, {
                rules: {
                    name: {
                        required: true
                    },
                    phone: {
                        required: true
                    },
                },
                submitHandler: async function (form) {
                    var $form = $(form);
                    var $btnSubmit = $form.find('button[type=submit]');
                    $btnSubmit.prop('disabled', true);

                    var { name, phone, email } = FormUtils.convertFormDataToDictionary($form.serializeArray());
                    try {
                        var rs = await ApiService.post('/api/receptionist/create', {
                            receptionist: {
                                fullName: name,
                                phoneNumber: phone,
                                email
                            }
                        });

                        if (rs && rs.receptionistId) {
                            $('.content-header').alert(true, 'success', 'Tạo thành công. Id: ' + rs.receptionistId);
                        } else {
                            $('.content-header').alert(true, 'danger', 'Tạo không thành công.');
                        }
                    } catch (ex) {
                        $('.content-header').alert(true, 'danger', ex);
                    } finally {
                        $btnSubmit.prop('disabled', false);
                    }
                }
            }))
    }
}
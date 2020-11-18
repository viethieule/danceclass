var m_packages = [];

$(async function () {
    initDatePicker();
    await populatePackages();
    registerEvent();
});

function initDatePicker() {
    $('#dob')
        .datepicker({
            autoclose: true,
            format: 'dd/mm/yyyy',
        })
        .inputmask('dd/mm/yyyy', {
            'placeholder': 'dd/mm/yyyy'
        });
}

async function populatePackages() {
    try {
        m_packages = await getPackages();

        var select = $('#package');
        m_packages.forEach(package => {
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

async function getPackages() {
    return $.ajax({
        method: 'GET',
        async: true,
        url: '/api/package/getDefaults'
    })
}

function registerEvent() {
    $('form#createMember')
        .submit(function (event) {
            event.preventDefault();
        })
        .validate({
            rules: {
                name: {
                    required: true
                },
                phone: {
                    required: true
                },
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
            highlight: function (element) {
                $(element).closest('.form-group').addClass('has-error');
            },
            unhighlight: function (element) {
                $(element).closest('.form-group').removeClass('has-error');
            },
            errorElement: 'span',
            errorClass: 'help-block',
            errorPlacement: function (error, element) {
                if (element.parent('.input-group').length) {
                    error.insertAfter(element.parent());
                } else {
                    error.insertAfter(element);
                }
            },
            submitHandler: async function (form) {
                var jForm = $(form);

                // Find disabled inputs, and remove the "disabled" attribute
                var disabled = jForm.find(':input:disabled').prop('disabled', false);
                // Get form data including disabled inputs
                var formData = FormUtils.convertFormDataToDictionary(jForm.serializeArray());
                // Re-disabled the set of inputs that were previously enabled
                disabled.prop('disabled', true);

                console.log(formData);

                try {
                    var package = m_packages.find(p => p.id.toString() === formData['package']);

                    let dob = $('#dob').datepicker('getDate');
                    if (dob) {
                        dob = moment(dob).format('MM-DD-YYYY');
                    }

                    const data = await createMember({
                        member: {
                            fullName: formData['name'],
                            birthdate: dob,
                            phoneNumber: formData['phone'],
                            email: formData['email']
                        },
                        package: package ? {
                            defaultPackageId: package.id
                        } : {
                            months: formData['expired'],
                            numberOfSessions: formData['sessions'],
                            price: formData['price'],
                        }
                    });
                    if (data && data.member) {
                        window.location.replace(window.location.origin + `/member/${data.member.userName}`);
                    }
                } catch (err) {
                    alert('Failed to create member');
                    console.log(err);
                }

                return false;
            }
        })

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
            var package = m_packages.find(p => p.id.toString() === packageId);
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

async function createMember(data) {
    return $.ajax({
        method: 'POST',
        async: true,
        url: '/api/user/create',
        data
    })
}

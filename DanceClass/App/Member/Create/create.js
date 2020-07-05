var m_packages = [];

$(async function () {
    await populatePackages();
    registerEvent();
});

async function populatePackages() {
    try {
        m_packages = await getPackages();
        console.log(m_packages);

        var select = $('#package');
        m_packages.forEach(package => {
            $('<option></option>', { value: package.Id })
                .text(package.NumberOfSessions)
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
        url: '/Services/Package/GetAll'
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
                dob: {
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
                debugger;
                var jForm = $(form);

                // Find disabled inputs, and remove the "disabled" attribute
                var disabled = jForm.find(':input:disabled').prop('disabled', false);
                // Get form data including disabled inputs
                var formData = convertFormDataToDictionary(jForm.serializeArray());
                // Re-disabled the set of inputs that were previously enabled
                disabled.prop('disabled', true);

                console.log(formData);

                try {
                    var package = m_packages.find(p => p.Id.toString() === formData['package']);

                    await createMember({
                        Member: {
                            FullName: formData['name'],
                            Birthdate: formData['dob'],
                            PhoneNumber: formData['phone'],
                            Email: formData['email']
                        },
                        Package: package || {
                            Months: formData['expired'],
                            NumberOfSessions: formData['sessions'],
                            Price: formData['price'],
                        }
                    });
                    alert('Create member successfully');
                } catch (err) {
                    alert('Failed to create member');
                    console.log(err);
                }

                return false;
            }
        })

    $('select#package')
        .on('change', function (e) {
            debugger;
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
            var package = m_packages.find(p => p.Id.toString() === packageId);
            var prop = fieldId === 'expired' ? "Months" : fieldId === 'sessions' ? "NumberOfSessions" : fieldId === 'price' ? "Price" : "";

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

function convertFormDataToDictionary(formData) {
    return formData.reduce((prev, cur) => {
        prev[cur.name] = cur.value;
        return prev;
    }, {});
}

async function createMember(data) {
    return $.ajax({
        method: 'POST',
        async: true,
        url: '/Services/Members/Create',
        data
    })
}

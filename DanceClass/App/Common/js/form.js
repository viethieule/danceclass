const jqueryValidateConfig = {
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
    }
}

function resetForm($form, excludes) {

    let inputElements = $form.find('input:text, input:password, input:file, input[type="number"], select, textarea');
    let checkedElements = $form.find('input:radio, input:checkbox');

    if (excludes && excludes.length > 0) {
        excludes.forEach(ex => {
            inputElements = inputElements.not(ex);
            checkedElements = checkedElements.not(ex);
        })
    }

    inputElements.val('');
    checkedElements.removeAttr('checked').removeAttr('selected');
}

function convertFormDataToDictionary(formData) {
    return formData.reduce((prev, cur) => {
        prev[cur.name] = cur.value;
        return prev;
    }, {});
}
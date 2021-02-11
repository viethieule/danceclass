function RevenueReportController() {
    this.initialize = function () {
        initDatePicker();
        initBranches();
        registerEvent();
    }

    function initDatePicker() {
        var start = new Date();
        start.setMonth(start.getMonth() - 1);
        $('#start').datePickerWithMask().datepicker('setDate', start);
        $('#end').datePickerWithMask().datepicker('setDate', new Date());
    }

    async function initBranches() {
        var rs = await ApiService.get('/api/branch/getall');
        if (rs && rs.branches) {
            '<label class="checkbox-inline"><input type="checkbox" value="1"> Quận 3</label>'
            rs.branches.forEach(function (branch) {
                var label = $('<label>', { class: 'checkbox-inline' });
                label.html(`<input class="checkbox-branch" type="checkbox" value="${branch.id}"> ${branch.name}`);
                $('.branches').append(label);
            });
        }
        $('.branches input:checkbox:not(:checked)').prop('checked', true);
    }

    function registerEvent() {
        registerFormEvent();
        registerFormSubmitEvent();
    }

    function registerFormEvent() {
        $('.allBranch').on('click', function (e) {
            var uncheckedOthers = $('.branches input:checkbox:not(:checked)').not('.allBranch');
            if (this.checked && uncheckedOthers.length > 0) {
                uncheckedOthers.prop('checked', true);
            } else if (!this.checked && uncheckedOthers.length === 0) {
                e.preventDefault();
                return false;
            }
        })

        $('.branches').on('click', '.checkbox-branch', function () {
            var isAllChecked = $('.allBranch').is(':checked');
            if (!this.checked && isAllChecked) {
                $('.allBranch').prop('checked', false);
            } else if (this.checked) {
                var others = $('.branches input:checkbox').not('.allBranch');
                if (others.length === $('.branches input:checkbox:checked').not('.allBranch').length && !isAllChecked) {
                    $('.allBranch').prop('checked', true);
                }
            }
        });
    }

    function registerFormSubmitEvent() {
        $('form#revenueReport')
            .submit(function (event) {
                event.preventDefault();
            })
            .validate(Object.assign(jqueryValidateConfig, {
                rules: {
                    start: {
                        required: true
                    },
                    end: {
                        required: true
                    },
                },
                submitHandler: async function (form) {
                    var $form = $(form);
                    var $btnSubmit = $form.find('button[type=submit]');
                    $btnSubmit.loading(true);

                    var start = $('#start').getInputDateString();
                    var end = $('#end').getInputDateString();
                    var isAllBranches = $('.allBranch').is(':checked');
                    var branchIds = [];
                    if (!isAllBranches) {
                        branchIds = $('.branches input:checkbox:checked')
                            .not('.allBranch')
                            .map(function () { return parseInt(this.value) })
                            .get();
                    }
                    var orderBy = parseInt($('#orderBy').val());

                    var data = {
                        start,
                        end,
                        branchIds,
                        orderBy
                    };

                    try {
                        var rs = await ApiService.post('/report/revenue', data);
                        if (rs && rs.Url) {
                            window.open(rs.Url, rs.IsRedirect ? '_self' : '_blank');
                        } else {
                            $('.content-header').alert(true, 'danger', 'An error has occurred');
                        }
                    } catch (ex) {
                        console.log(ex);
                        $('.content-header').alert(true, 'danger', ex);
                    } finally {
                        $btnSubmit.loading(false);
                    }
                }
            }))
    }
}
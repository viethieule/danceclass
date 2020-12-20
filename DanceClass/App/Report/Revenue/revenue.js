function RevenueReportController() {
    this.initialize = function () {
        initDatePicker();
        registerEvent();
    }

    function initDatePicker() {
        var start = new Date();
        start.setMonth(start.getMonth() - 1);
        $('#start').datePickerWithMask().datepicker('setDate', start);
        $('#end').datePickerWithMask().datepicker('setDate', new Date());
    }

    function registerEvent() {
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
                    $btnSubmit.prop('disabled', true);

                    var start = $('#start').getInputDateString();
                    var end = $('#end').getInputDateString();

                    try {
                        var rs = await ApiService.post('/api/report/revenuereport', { start, end });
                        if (rs && rs.url) {
                            window.open(rs.url, '_blank');
                        } else {
                            $('.content-header').alert(true, 'danger', 'An error has occurred');
                        }
                    } catch (ex) {
                        console.log(ex);
                        $('.content-header').alert(true, 'danger', ex);
                    } finally {
                        $btnSubmit.prop('disabled', false);
                    }
                }
            }))
    }

    $.fn.getInputDateString = function () {
        let date = this.datepicker('getDate');
        if (date) {
            date = moment(date).format('MM-DD-YYYY');
        }
        return date;
    }
}
(function ($) {
    $.fn.alert = function (isShow, error) {
        var message = '';
        if (typeof error === 'string' || error instanceof String) {
            message = error;
        } else if (error) {
            message = error.responseJSON ? error.responseJSON.ExceptionMessage : (error.Message || error.Message || 'Đã có lỗi xảy ra');
        }

        var $alert = this.find('.alert');
        if ($alert.length === 0 && isShow) {
            $alert = $('<div>', { class: 'alert alert-danger alert-dismissible' })
                .append(
                    $('<button>', { type: 'button', class: 'close', 'data-dismiss': 'alert', 'aria-hidden': 'true' }).html('&times;')
                )
                .append(
                    $('<h4><i class="icon fa fa-ban"></i> Alert!</h4>')
                )
                .append(
                    $('<p>').html(message)
                );
        } else if (message) {
            $alert.find('p').html(message);
        }
        $alert.toggle(isShow);
        return this;
    }
}(jQuery));
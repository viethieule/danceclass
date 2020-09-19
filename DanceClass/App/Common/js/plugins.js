﻿(function ($) {
    $.fn.alert = function (isShow, style, error, fadeTime) {
        var alertStyleMap = [
            { name: 'danger', class: 'alert-danger', icon: 'fa-ban', title: 'Lỗi' },
            { name: 'info', class: 'alert-info', icon: 'fa-info', title: 'Lưu ý' },
            { name: 'warning', class: 'alert-warning', icon: 'fa-warning', title: 'Chú ý!' },
            { name: 'success', class: 'alert-success', icon: 'fa-check', title: 'Thành công' },
        ]

        var message = '';
        if (typeof error === 'string' || error instanceof String) {
            message = error;
        } else if (error) {
            message = error.responseJSON ? error.responseJSON.ExceptionMessage : (error.Message || error.Message || 'Đã có lỗi xảy ra');
        }

        var alertStyle = alertStyleMap.find(s => s.name === style);

        var $alert = this.find(alertStyle ? ('.' + alertStyle.class) : '.alert');
        if ($alert.length === 0 && isShow) {
            if (!alertStyle) {
                alertStyle = alertStyleMap.find(s => s.name === 'info');
            }

            $alert = $('<div>', { class: 'alert ' + alertStyle.class + ' alert-dismissible' })
                .append(
                    $('<button>', { type: 'button', class: 'close', 'data-dismiss': 'alert', 'aria-hidden': 'true' }).html('&times;')
                )
                .append(
                    $('<h4><i class="icon fa ' + alertStyle.icon + '"></i> ' + alertStyle.title + '</h4>')
                )
                .append(
                    $('<p>').html(message)
                );

            $alert.prependTo(this);
        } else if (message) {
            $alert.find('p').html(message);
        }
        $alert.toggle(isShow);

        if (isShow && fadeTime) {
            setTimeout(function () {
                $alert.toggle(false);
            }, fadeTime);
        }

        return this;
    }

    $.fn.appendOptions = function (data, valueProp, textProp) {
        if (data && data.length === 0) {
            return this;
        }

        var $this = this;
        $this.empty();
        data.forEach(function (item, i, a) {
            var attr = { value: item[valueProp] };
            if (i === 0) { attr.selected = 'selected' }
            $('<option>', attr).text(item[textProp]).appendTo($this);
        });

        return $this;
    }
}(jQuery));
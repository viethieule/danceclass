const apiService = (function () {

    function ajax(method, url, data) {
        let request = { method, url }
        if (data) request.data = data;

        return $.ajax(request);
    }

    function get(url) {
        return ajax('GET', url);
    }

    function post(url, data) {
        return ajax('POST', url, data);
    }

    function put(url) {
        return ajax('PUT', url);
    }

    function del(url) {
        return ajax('DELETE', url);
    }

    return {
        get, post, put, del
    }
})();
const ApiService = (function () {

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

    function fileDownload(url, data, errorCallback) {
        $.ajax({
            type: "POST",
            url,
            data,
            xhrFields: {
                responseType: 'blob' // to avoid binary data being mangled on charset conversion
            },
            success: function (blob, status, xhr) {
                // check for a filename
                var filename = "";
                var disposition = xhr.getResponseHeader('Content-Disposition');
                if (disposition && disposition.indexOf('attachment') !== -1) {
                    var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                    var matches = filenameRegex.exec(disposition);
                    if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                }

                if (typeof window.navigator.msSaveBlob !== 'undefined') {
                    // IE workaround for "HTML7007: One or more blob URLs were revoked by closing the blob for which they were created. These URLs will no longer resolve as the data backing the URL has been freed."
                    window.navigator.msSaveBlob(blob, filename);
                } else {
                    var URL = window.URL || window.webkitURL;
                    var downloadUrl = URL.createObjectURL(blob);

                    if (filename) {
                        // use HTML5 a[download] attribute to specify filename
                        var a = document.createElement("a");
                        // safari doesn't support this yet
                        if (typeof a.download === 'undefined') {
                            window.location.href = downloadUrl;
                        } else {
                            a.href = downloadUrl;
                            a.download = filename;
                            document.body.appendChild(a);
                            a.click();
                        }
                    } else {
                        window.location.href = downloadUrl;
                    }

                    setTimeout(function () { URL.revokeObjectURL(downloadUrl); }, 100); // cleanup
                }
            },
            error: function (jqXHR, textStatus, errorThrow) {
                console.log(jqXHR);
                errorCallback(errorThrow);
            }
        });
    }

    return {
        get, post, put, del, fileDownload
    }
})();
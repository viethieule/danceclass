function GetAllController() {
    this.initialize = function () {
        initDatePicker();
        populatePackages();
        populateMembers();
        registerEvent();
    }

    function initDatePicker() {
        $('#createdDateFrom').datePickerWithMask()
        $('#createdDateTo').datePickerWithMask()
        $('#expiryDateFrom').datePickerWithMask()
        $('#expiryDateTo').datePickerWithMask()
    }

    function registerEvent() {
        $('#search').on('click', function () {
            $('#members').DataTable().ajax.reload();
        })

        $('.btn-toggle-search').on('click', function () {
            var btn = $(this);
            var searchContainer = $('.search-container');
            searchContainer.slideToggle(400, 'swing', function () {
                btn.html(searchContainer.is(':visible') ? 'Ẩn tìm kiếm' : 'Hiện tìm kiếm');
            });
        })
    }

    async function populatePackages() {
        try {
            var defaultPackages = await ApiService.get('/api/package/getDefaults');

            var select = $('#defaultPackageId');
            select.append($('<option>'));
            defaultPackages.forEach(package => {
                $('<option></option>', { value: package.id })
                    .text(package.numberOfSessions + ' buổi')
                    .appendTo(select);
            });

            $('<option></option>', { value: -1 })
                .text('Khác...')
                .appendTo(select);
        } catch (err) {
            console.log(err);
            $('.content-header').alert(true, 'danger', err);
        }
    }

    function populateMembers() {
        $('#members').dataTable({
            ajax: {
                url: '/api/user/getAllMember',
                type: 'POST',
                data: {
                    name: function () { return $('#name').val() },
                    phoneNumber: function () { return $('#phoneNumber').val() },
                    defaultPackageId: function () { return $('#defaultPackageId').val() },
                    createdDateFrom: function () { return $('#createdDateFrom').getInputDateString() },
                    createdDateTo: function () { return $('#createdDateTo').getInputDateString() },
                    expiryDateFrom: function () { return $('#expiryDateFrom').getInputDateString() },
                    expiryDateTo: function () { return $('#expiryDateTo').getInputDateString() },
                },
                dataSrc: 'members',
                error: function (xhr, textStatus, error) {
                    console.log(textStatus);
                    $('.content-header').alert(true, 'danger', textStatus);
                }
            },
            columns: [
                {
                    title: 'Tên',
                    data: function (row, type, val, meta) {
                        return {
                            userName: row.userName,
                            fullName: row.fullName
                        };
                    },
                    render: function (data, type, row, meta) {
                        return `<a href="/member/${data.userName}">${data.fullName}</a>`;
                    },
                },
                { title: 'Số điện thoại', data: 'phoneNumber' },
                { title: 'Số buổi còn lại', data: 'membership.remainingSessions' },
                {
                    title: 'Ngày tạo',
                    data: 'createdDate',
                    render: function (data, type, row, meta) {
                        return moment(data).format('DD/MM/YYYY')
                    }
                },
                {
                    title: 'Ngày hết hạn',
                    data: 'membership.expiryDate',
                    render: function (data, type, row, meta) {
                        return moment(data).format('DD/MM/YYYY')
                    }
                },
            ],
            scrollX: true,
            pageLength: 10,
            ordering: false,
            searching: true,
            language: {
                lengthMenu: 'Hiển thị _MENU_ hội viên trên trang',
                info: 'Trang _PAGE_ trên _PAGES_',
                zeroRecords: 'Chưa có hội viên nào đăng ký',
                infoEmpty: '',
                search: '',
                searchPlaceholder: 'Tìm kiếm trên kết quả...',
                paginate: {
                    previous: 'Trước',
                    next: 'Sau'
                }
            }
        });
    }
}


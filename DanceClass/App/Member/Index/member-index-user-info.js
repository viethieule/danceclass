function UserInfoController() {
    var _self = this;

    this.initUserInfo = async function () {
        await _self.loadUser();
        loadUserRegistrations();
    }

    this.loadUser = async function () {
        const fragments = window.location.pathname.split('/');
        const username = fragments[fragments.length - 1];
        try {
            _self.member = _self.member || await UserService.get({ username });
            if (_self.member) {
                const { userName, birthdate, fullName, phoneNumber, membership } = _self.member;
                $('#username').text(userName);
                $('#fullName').text(fullName);
                $('#birthdate').text(moment(birthdate).format('DD/MM/YYYY'));
                $('#phoneNumber').text(phoneNumber);
                if (UserService.isAdmin()) {
                    $('#changePassword').attr('href', '/Manage/ChangePassword?userId=' + _self.member.id);
                } else {
                    $('#changePassword').attr('href', '/Manage/ChangePassword');
                }

                _self.loadMembershipData();
                _self.loadMemberPackages();
            } else {
                $('.content-header').alert(true, 'danger', 'Không tìm thấy');
            }
        } catch (ex) {
            $('.content-header').alert(true, 'danger', ex);
        }
    }

    async function loadUserRegistrations() {
        var { member } = _self;
        if (!member) {
            return;
        }

        try {
            const rs = await ApiService.post('/api/registration/getByUser', { userId: member.id });
            if (rs && rs.registrations) {
                const { registrations } = rs;
                const dataSet = registrations.reduce(function (result, registration) {
                    const {
                        scheduleDetail: {
                            date: dateStr,
                            schedule: {
                                class: {
                                    name: className
                                },
                                trainer: {
                                    name: trainerName
                                },
                                branch,
                                startTime,
                                song
                            }
                        },
                        status: {
                            name: statusText
                        }
                    } = registration;

                    const [hour, minute] = startTime.split(':');
                    date = new Date(dateStr);
                    date.setHours(hour);
                    date.setMinutes(minute);

                    let data = [className, song, trainerName, moment(date).format('DD/MM/YYYY HH:mm'), new Date(date), branch, statusText];
                    result.push(data);
                    return result;
                }, []);

                $('#registrations').DataTable({
                    data: dataSet,
                    columns: [
                        { title: "Lớp", orderable: false },
                        { title: "Bài múa", orderable: false },
                        { title: "Giáo viên", orderable: false, width: '11%' },
                        { title: "Ngày", orderData: 4, width: '18%' },
                        { visible: false },
                        { title: "Chi nhánh", orderable: false, width: '11%' },
                        { title: "Trạng thái", orderable: false, width: '11%' }
                    ],
                    scrollX: true,
                    order: [[4, 'desc']],
                    autoWidth: false,
                    lengthMenu: [10, 20, 30],
                    language: {
                        lengthMenu: 'Hiển thị _MENU_ đăng ký trên trang',
                        info: 'Trang _PAGE_ trên _PAGES_',
                        zeroRecords: 'Chưa đăng ký buổi học nào',
                        infoEmpty: '',
                        search: 'Tìm kiếm',
                        paginate: {
                            previous: 'Trước',
                            next: 'Sau'
                        }
                    }
                })
            }
        } catch (ex) {
            $('.content-header').alert(true, 'danger', ex);
        }
    }
}
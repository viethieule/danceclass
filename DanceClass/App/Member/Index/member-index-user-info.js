function UserInfoController() {
    var _self = this;

    this.initUserInfo = async function () {
        initUi();
        registerEvent();
        await _self.loadUser();
        loadUserRegistrations();
    }

    this.loadUser = async function () {
        const fragments = window.location.pathname.split('/');
        const username = fragments[fragments.length - 1];
        try {
            _self.member = _self.member || await UserService.get({ username });
            if (_self.member) {
                populateUserInfo(_self.member);

                if (_self.member.id === _self.currentUser.id) {
                    $('#changePassword').attr('href', '/Manage/ChangePassword');
                } else if (UserService.isAdmin()) {
                    $('#changePassword').attr('href', '/Manage/ChangePassword?userId=' + _self.member.id);
                } else {
                    $('#changePassword').parent().remove();
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

    function populateUserInfo(user) {
        const { userName, birthdate, fullName, phoneNumber, registeredBranch } = user;
        $('#username').text(userName);
        $('#fullName').text(fullName);
        $('#birthdate').text(birthdate ? moment(birthdate).format('DD/MM/YYYY') : '');
        $('#phoneNumber').text(phoneNumber);
        $('#branch').text(registeredBranch ? registeredBranch.name : '');

        $('#editFullName').val(fullName);
        $('#editPhoneNumber').val(phoneNumber);
        $('#editUserName').val(userName);
        if (birthdate) {
            $('#editBirthdate').datepicker('setDate', new Date(birthdate));
        }
    }

    function initUi() {
        $('#editBirthdate').datePickerWithMask();
    }

    function registerEvent() {
        $('#editUserInfo')
            .submit(function (event) {
                event.preventDefault();
            })
            .validate(Object.assign(jqueryValidateConfig, {
                rules: {
                    name: {
                        required: true,
                        noSpace: true,
                    },
                    username: {
                        required: true,
                        noSpace: true,
                    },
                    phone: {
                        required: true,
                        noSpace: true,
                    }
                },
                submitHandler: async function (form) {
                    $modal = $('#modal-edit-user-info');

                    var formData = FormUtils.convertFormDataToDictionary($(form).serializeArray());
                    let birthdate = $('#editBirthdate').getInputDateString()
                    try {
                        var userName = formData['username'] ? formData['username'].trim() : '';
                        var isUsernameChanged = _self.member.userName !== userName;
                        var rs = await ApiService.post('/api/user/edit', {
                            id: _self.member.id,
                            fullName: formData['name'],
                            userName: userName,
                            birthdate: birthdate,
                            phoneNumber: formData['phone']
                        });
                        if (rs && rs.member) {
                            populateUserInfo(rs.member);
                            _self.member.userName = rs.member.userName;
                            _self.member.fullName = rs.member.fullName;
                            _self.member.phoneNumber = rs.member.phoneNumber;
                            if (isUsernameChanged) {
                                const fragments = window.location.pathname.split('/');
                                fragments[fragments.length - 1] = userName;
                                var newPathName = fragments.join('/');

                                if (history.pushState) {
                                    var newUrl = window.location.protocol + "//" + window.location.host + newPathName;
                                    window.history.pushState({ path: newUrl }, '', newUrl);
                                }
                            }
                            $('.content-header').alert(true, 'success', 'Sửa thành công', 2000);
                        } else {
                            $('.content-header').alert(true, 'danger', 'Đã có lỗi xảy ra');
                        }
                    } catch (ex) {
                        console.log(ex);
                        $('.content-header').alert(true, 'danger', ex);
                    } finally {
                        $modal.modal('hide');
                    }
                }
            }));

        $('#deleteUser').on('click', function () {
            try {
                _self.showAlert('Bạn có chắc chắn muốn xóa hội viên này?', async function () {
                    var rs = await ApiService.del('/api/user/delete/' + _self.member.id);
                    if (rs && rs.success) {
                        $('.content-header').alert(true, 'success', 'Xóa thành công');
                        setTimeout(function () {
                            window.location = '/';
                        }, 1500);
                    }
                });                
            } catch (ex) {
                console.log(ex);
                $('.content-header').alert(true, 'danger', ex);
            }
        })
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
let m_user = null;

$(async function () {
    await populateUserInfo();
    await populateRegistrations();
});

async function populateUserInfo() {
    const fragments = window.location.pathname.split('/');
    const username = fragments[fragments.length - 1];
    try {
        m_user = await UserService.get({ username });
        if (m_user) {
            console.log(m_user);
            const { userName, birthdate, fullName, phoneNumber } = m_user;
            $('#username').text(userName);
            $('#fullName').text(fullName);
            $('#birthdate').text(moment(birthdate).format('DD/MM/YYYY'));
            $('#phoneNumber').text(phoneNumber);
        } else {
            showErrorMessage('Không tìm thấy!');
        }
    } catch (ex) {
        handleAjaxError(ex);
    }
}

async function populateRegistrations() {
    if (!m_user) {
        return;
    }

    try {
        const rs = await ApiService.post('/api/registration/getByUser', { userId: m_user.id });
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
        handleAjaxError(ex);
    }
}

function handleAjaxError(ex) {
    console.log(ex);
    if (ex.responseJSON) {
        if (ex.responseJSON.ExceptionMessage) {
            showErrorMessage(ex.responseJSON.ExceptionMessage);
        } else if (ex.responseJSON.Message) {
            showErrorMessage(ex.responseJSON.Message);
        }
    } else {
        showErrorMessage('Một lỗi nào đó đã xảy ra :(');
    }
}

function showErrorMessage(message) {
    $('#global-error-message').empty().text(message);
    $('.alert').show();
}
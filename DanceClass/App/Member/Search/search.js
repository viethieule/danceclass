function SearchController() {
    this.initialize = function () {
        populateMembers();
    }

    async function populateMembers() {
        const searchParams = new URLSearchParams(window.location.search);
        const query = searchParams.get('search');

        try {
            var result = await ApiService.post('/api/user/searchMember', { query });
            if (result && result.members && result.members.length) {
                var { members } = result;

                members.forEach(function (member) {
                    $('<tr>')
                        .append($('<td>').text(member.fullName))
                        .append($('<td>').text(member.phoneNumber))
                        .append($('<td>').text(member.membership.remainingSessions))
                        .append($('<td>').text(member.membership.expiryDate))
                        .appendTo($('#members'))
                });
            } else {
                $('<tr>')
                    .append($('<td colspan="4">').text('Không tìm thấy hoặc chuỗi tìm kiếm quá ngắn'))
                    .appendTo($('#members'))
            }
        } catch (ex) {
            console.log(ex);
            $('.content-header').alert(true, 'danger', ex);
        } finally {
            $('.spinner').hide();
        }
    }
}
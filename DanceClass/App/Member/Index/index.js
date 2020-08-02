$(async function () {
    const member = await getMember();
    $('#username').text(member.userName);
});

async function getMember() {
    const fragments = window.location.pathname.split('/');
    const username = fragments[fragments.length - 1];
    try {
        const data = await ajaxMember(username);
        console.log(data);
        return data.member;
    } catch (err) {
        console.log(err);
    }
}

async function ajaxMember(username) {
    return $.ajax({
        method: 'POST',
        data: { username },
        async: true,
        url: '/api/user/get',
    });
}
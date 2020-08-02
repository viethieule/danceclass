$(async function () {
    const member = await getMember();
    $('#username').text(member.userName);
});

async function getMember() {
    const fragments = window.location.pathname.split('/');
    const username = fragments[fragments.length - 1];
    try {
        const member = await userService.get({ username });
        console.log(member);
        return member;
    } catch (err) {
        console.log(err);
    }
}
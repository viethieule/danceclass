async function getCurrentUser() {
    const user = await ajaxCurrentUser();
    return user;
}

function ajaxCurrentUser() {
    return $.ajax({
        method: 'POST',
        async: true,
        url: '/Services/Members/GetCurrentUser'
    });
}
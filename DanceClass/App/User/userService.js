const userService = (function () {
    let user = null;

    async function getCurrentUser() {
        if (user === null) {
            const data = await $.ajax({
                method: 'POST',
                async: true,
                url: '/Services/Members/GetCurrentUser'
            });

            if (data) {
                user = data.Member;
            }
        }

        return user;
    }

    return {
        getCurrentUser: getCurrentUser
    }
})();
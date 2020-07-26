const userService = (function () {
    var user = null;

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

    async function isAdmin() {
        return (await isInRole('Admin'));
    }

    async function isMember() {
        return (await isInRole('Member'));
    }

    async function isInRole(roleName) {
        await getCurrentUser();
        return user && user.RoleNames.some(r => r === roleName);
    }

    return {
        getCurrentUser: getCurrentUser,
        isAdmin: isAdmin,
        isMember: isMember
    }
})();
const userService = (function () {
    let user = null;

    async function getCurrentUser() {
        if (user === null) {
            const data = await $.ajax({
                method: 'POST',
                async: true,
                url: '/api/user/getCurrentUser'
            });

            if (data) {
                user = data.member;
            }
        }

        return user;
    }

    function isAdmin() {
        return isInRole('Admin');
    }

    function isMember() {
        return isInRole('Member');
    }

    function isInRole(roleName) {
        return user && user.roleNames.includes(roleName);
    }

    return {
        getCurrentUser: getCurrentUser,
        isAdmin: isAdmin,
        isMember: isMember
    }
})();
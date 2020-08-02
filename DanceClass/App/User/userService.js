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

    async function get(rq) {
        const rs = await $.ajax({
            method: 'POST',
            data: rq,
            async: true,
            url: '/api/user/get'
        });

        if (rs && rs.member) {
            return rs.member;
        }

        return null;
    }

    return {
        getCurrentUser: getCurrentUser,
        isAdmin: isAdmin,
        isMember: isMember,
        get: get
    }
})();
const UserService = (function () {
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

    function isReceptionist() {
        return isInRole('Receptionist');
    }

    function isCollaborator() {
        return isInRole('Collaborator');
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

    async function search(query) {
        const rs = await ApiService.post('/api/user/searchMember', { query });

        if (rs && rs.members) {
            return rs.members;
        }

        return [];
    }

    return {
        getCurrentUser: getCurrentUser,
        isAdmin: isAdmin,
        isMember: isMember,
        isReceptionist: isReceptionist,
        isCollaborator: isCollaborator,
        get: get,
        search: search
    }
})();
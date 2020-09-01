const Utils = (function () {
    function collapseSideBar() {
        $('body').removeClass('sidebar-collapse').addClass('sidebar-collapse');
    }

    function capitalizeFirstLetter(string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }

    function formatHhMm(hours, minutes) {
        return `${formatLeadingZero(hours)}:${formatLeadingZero(minutes)}`;
    }

    function formatLeadingZero(timePart) {
        timePart = timePart.toString();
        return timePart.length === 1 ? '0' + timePart : timePart;
    }

    function setNavBarTitle() {
        $('.navbar-current-page-title').text($('.content-header-title').text())
    }

    return {
        collapseSideBar,
        setNavBarTitle,
        capitalizeFirstLetter,
        formatHhMm
    }
})();
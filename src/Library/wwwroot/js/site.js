var utils = {
    blur: function (element) {
        element.blur();
    },
    refocus: function () {
        $('[autofocus]').focus()
    },
    collapseNavbar: function () {
        $(".navbar-collapse").collapse('hide');
    }
}
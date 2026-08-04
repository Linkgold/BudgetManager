window.selectAllText = function (elementId) {
    const element = document.getElementById(elementId);

    if (element) {
        element.select();
    }
};
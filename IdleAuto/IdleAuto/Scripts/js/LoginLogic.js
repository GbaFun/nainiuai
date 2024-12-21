
CefSharp.BindObjectAsync('Bridge');

function showMessage(message) {
    alert(message);
}

function getMessage() {
    alert("1111111getMessage", function () {
        var msg = Bridge.getMessage();
        alert(msg);
    });
}
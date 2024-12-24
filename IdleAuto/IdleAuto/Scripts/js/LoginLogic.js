

async function init() {
    await CefSharp.BindObjectAsync('Bridge');
    onInitSuccess();
}

function onInitSuccess() {
    console.log("onInitSuccess");
}

init();

function showMessage(message) {
    alert(message);
}

function getMessage() {
    alert("1111111getMessage", function () {
        var msg = Bridge.getMessage();
        alert(msg);
    });
}


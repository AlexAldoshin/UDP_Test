
$(function () {
    InitEvents();
    //$('#R1ON').show(200);
    //$('#R1OFF').show(300);
    //$('#R2ON').show(400);
    //$('#R2OFF').show(500);
    
});

function InitEvents() {        
    $.getJSON('/Home/RelayGet', RelaySetSuccess);
    loadData();   
    
    $('#R1ON').click({ relay: 1, set: 1 }, RelaySet); // на кнопку прикрутим обработчик
    $('#R1OFF').click({ relay: 1, set: 0 }, RelaySet); // на кнопку прикрутим обработчик
    $('#R2ON').click({ relay: 2, set: 1 }, RelaySet); // на кнопку прикрутим обработчик
    $('#R2OFF').click({ relay: 2, set: 0 }, RelaySet); // на кнопку прикрутим обработчик

    setInterval('loadData()', 5000);
}
function RelaySet(eventObj) {  

    $.getJSON('/Home/RelaySet', { relay: eventObj.data.relay, set: eventObj.data.set }, RelaySetSuccess);
}
function RelaySetSuccess(data) {
    if (data.Rel1 == 0) {
        $('#R1ON').show(100);
        $('#R1OFF').hide(100);
    }
    else {
        $('#R1ON').hide(100);
        $('#R1OFF').show(100);
    }
    if (data.Rel2 == 0) {
        $('#R2ON').show(100);
        $('#R2OFF').hide(100);
    }
    else {
        $('#R2ON').hide(100);
        $('#R2OFF').show(100);
    }
}
function loadData() {
    $.getJSON('/Home/LastDataGet', LastDataGetSuccess);
}
function LastDataGetSuccess(data) {
    $('#BatLev').text("Напряжение батареи: " + data.LastData[45]/10 + " Вольт");
}
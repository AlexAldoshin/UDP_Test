
$(function () {
    InitEvents();
});

function InitEvents() {
    $('#R1ON').click({ relay: 1, set: 1 }, RelaySet); // на кнопку прикрутим обработчик
    $('#R1OFF').click({ relay: 1, set: 0 }, RelaySet); // на кнопку прикрутим обработчик
    $('#R2ON').click({ relay: 2, set: 1 }, RelaySet); // на кнопку прикрутим обработчик
    $('#R2OFF').click({ relay: 2, set: 0 }, RelaySet); // на кнопку прикрутим обработчик
}

function RelaySet(eventObj) {  

    $.get('/Home/RelaySet', { relay: eventObj.data.relay, set: eventObj.data.set});
}
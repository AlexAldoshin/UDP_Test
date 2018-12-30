

var myMap;
var SetCurrentPos=0;
var DevLocation;
// Дождёмся загрузки API и готовности DOM.
ymaps.ready(init);

function init() {
    // Создание экземпляра карты и его привязка к контейнеру с
    // заданным id ("map").
    myMap = new ymaps.Map('map', {
        // При инициализации карты обязательно нужно указать
        // её центр и коэффициент масштабирования.
        center: [55.76, 37.64], // Москва
        zoom: 10
    }, {
            searchControlProvider: 'yandex#search'
        });
    DevLocation = new ymaps.Placemark([0, 0]);
    myMap.geoObjects.add(DevLocation);
}



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

    var text = "<p>Напряжение батареи: " + data.Ubat / 10 + " Вольт</p>";
    text += "<p>Latitude:" + data.Latitude + "</p>";
    text += "<p>Longitude:" + data.Longitude + "</p>";
    text += "<p>Altitude:" + data.MSL_Altitude + "</p>";
    text += "<p>Скорость:" + data.Speed_Over_Ground + "</p>";
    text += "<p>Курс:" + data.Course_Over_Ground + "</p>";
    text += "<p>Точность:" + ((data.HDOP + data.PDOP + data.VDOP) / 3).toFixed(1)  + "</p>";
    $('#BatLev').html(text);


    if (SetCurrentPos == 0) {
        myMap.setCenter([data.Latitude, data.Longitude]);
        myMap.setZoom(18);
        SetCurrentPos = data.Latitude + data.Longitude;
    }    
    DevLocation.geometry.setCoordinates([data.Latitude, data.Longitude]);


    
}
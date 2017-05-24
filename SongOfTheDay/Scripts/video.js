
//$('.stream-video').click(function ()
//{
//    $('.video-link').val('');
//});
$(function()
{
    var name = $('.archive-video-information').first().text();
    var info = $('.archive-video-information-info').first().html();
    $('.info-name').text(name);
    $('.info-info').html(info);
});

$(document).on('click', '.archive-video', function ()
{
    var name = $(this).find('.archive-video-information').text();
    var info = $(this).find('.archive-video-information-info').html();
    $('.info-name').text(name);
    $('.info-info').html(info);
    var src = $(this).attr('id');
    console.log($('.video > iframe'));
    $('.video > iframe').attr('src', src);
});

$(document).on('click', '.suggested-video-submit', function()
{
    var url = $('.suggested-video-url').val();

    if ((url.indexOf("www.youtube.com") > -1 || url.indexOf("www.youtu.be") > -1 || url.indexOf("soundcloud") > -1) && url.length >= 27)
    {
        $('.suggestion-from').append('<div class="suggestion suggestion-added">Suggestion Added!</div>');
    }
    else
    {
        $('.suggestion-from').append('<div class="suggestion suggestion-not-added">Bad Url!</div>');
    }
});
(function ($) {

	"use strict";

	// Window Resize Mobile Menu Fix
	mobileNav();


	// Scroll animation init
	window.sr = new scrollReveal();


	// Menu Dropdown Toggle
	if ($('.menu-trigger').length) {
		$(".menu-trigger").on('click', function () {
			$(this).toggleClass('active');
			$('.header-area .nav').slideToggle(200);
		});
	}


	// Menu elevator animation (Primer handler - este ya estaba seguro)
	$('a[href*=\\#]:not([href=\\#])').on('click', function () {
		if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
			var target = $(this.hash);
			target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
			if (target.length) {
				var width = $(window).width();
				if (width < 991) {
					$('.menu-trigger').removeClass('active');
					$('.header-area .nav').slideUp(200);
				}
				$('html,body').animate({
					scrollTop: (target.offset().top) - 130
				}, 700);
				return false;
			}
		}
	});

	$(document).ready(function () {
		$(document).on("scroll", onScroll);

		//smoothscroll (Segundo handler - CORREGIDO)
		$('a[href^="#"]').on('click', function (e) {
			// CORRECCIÓN: Si el hash es solo #, no hacer nada.
			if (this.hash === "" || this.hash === "#") {
				return;
			}

			e.preventDefault();
			$(document).off("scroll");

			$('a').each(function () {
				$(this).removeClass('active');
			})
			$(this).addClass('active');

			var targetHash = this.hash;
			var target = $(targetHash); // Esto ahora es seguro por el if de arriba

			// Verificamos si target realmente existe antes de animar
			if (target.length) {
				$('html, body').stop().animate({
					scrollTop: (target.offset().top) - 130
				}, 500, 'swing', function () {
					window.location.hash = targetHash;
					$(document).on("scroll", onScroll);
				});
			} else {
				// Si no existe el destino, reactivamos el scroll
				$(document).on("scroll", onScroll);
			}
		});
	});

	// Función onScroll CORREGIDA
	function onScroll(event) {
		var scrollPos = $(document).scrollTop();
		$('.nav a').each(function () {
			var currLink = $(this);
			var href = currLink.attr("href");

			// CORRECCIÓN IMPORTANTE: 
			// Validamos que href exista, empiece con # y tenga más de 1 caracter (ej. "#inicio" es válido, "#" no lo es)
			if (!href || href === "#" || !href.startsWith("#")) {
				return; // Saltamos esta iteración si el enlace no es válido para scroll
			}

			// Usamos try-catch por seguridad adicional si el selector es inválido por otros caracteres
			try {
				var refElement = $(href);
				if (refElement.length && refElement.position().top <= scrollPos && refElement.position().top + refElement.height() > scrollPos) {
					$('.nav ul li a').removeClass("active");
					currLink.addClass("active");
				}
				else {
					currLink.removeClass("active");
				}
			} catch (e) {
				// Si falla el selector, ignoramos este elemento
			}
		});
	}


	// Home seperator
	if ($('.home-seperator').length) {
		$('.home-seperator .left-item, .home-seperator .right-item').imgfix();
	}


	// Home number counterup
	if ($('.count-item').length) {
		$('.count-item strong').counterUp({
			delay: 10,
			time: 1000
		});
	}


	// Page loading animation
	$(window).on('load', function () {
		if ($('.cover').length) {
			$('.cover').parallax({
				imageSrc: $('.cover').data('image'),
				zIndex: '1'
			});
		}

		$("#preloader").animate({
			'opacity': '0'
		}, 600, function () {
			setTimeout(function () {
				$("#preloader").css("visibility", "hidden").fadeOut();
			}, 300);
		});
	});


	// Window Resize Mobile Menu Fix
	$(window).on('resize', function () {
		mobileNav();
	});


	// Window Resize Mobile Menu Fix
	function mobileNav() {
		var width = $(window).width();
		$('.submenu').on('click', function () {
			if (width < 992) {
				$('.submenu ul').removeClass('active');
				$(this).find('ul').toggleClass('active');
			}
		});
	}


})(window.jQuery);
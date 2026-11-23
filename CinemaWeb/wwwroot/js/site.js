/* ARDÓSIA CINEMAS — acessibilidade, busca com sugestões e pequenos ajudantes */
(function () {
    'use strict';

    var raiz = document.documentElement;
    var CHAVE_TAMANHO = 'ardosia.tamanho';
    var CHAVE_CONTRASTE = 'ardosia.contraste';
    var MIN = 80, MAX = 160, PASSO = 10, PADRAO = 100;

    function ler(chave) { try { return localStorage.getItem(chave); } catch (e) { return null; } }
    function gravar(chave, valor) { try { localStorage.setItem(chave, valor); } catch (e) { } }

    // ---------------------------------------------------- Tamanho do texto
    var tamanho = parseInt(ler(CHAVE_TAMANHO) || PADRAO, 10);
    if (isNaN(tamanho)) tamanho = PADRAO;

    function aplicarTamanho() {
        raiz.style.fontSize = tamanho + '%';
        gravar(CHAVE_TAMANHO, tamanho);
    }

    // ---------------------------------------------------- Contraste
    function contrasteAtivo() { return raiz.classList.contains('alto-contraste'); }

    function sincronizarBotaoContraste() {
        var botao = document.querySelector('[data-acao="contraste"]');
        if (botao) botao.setAttribute('aria-pressed', contrasteAtivo() ? 'true' : 'false');
    }

    document.addEventListener('click', function (e) {
        var botao = e.target.closest ? e.target.closest('[data-acao]') : null;
        if (!botao) return;

        switch (botao.getAttribute('data-acao')) {
            case 'aumentar': tamanho = Math.min(MAX, tamanho + PASSO); aplicarTamanho(); break;
            case 'diminuir': tamanho = Math.max(MIN, tamanho - PASSO); aplicarTamanho(); break;
            case 'resetar': tamanho = PADRAO; aplicarTamanho(); break;
            case 'contraste':
                raiz.classList.toggle('alto-contraste');
                gravar(CHAVE_CONTRASTE, contrasteAtivo() ? '1' : '0');
                sincronizarBotaoContraste();
                break;
        }
    });
    sincronizarBotaoContraste();

    // ---------------------------------------------------- Confirmação antes de excluir/cancelar
    document.addEventListener('submit', function (e) {
        var msg = e.target.getAttribute && e.target.getAttribute('data-confirmar');
        if (msg && !window.confirm(msg)) e.preventDefault();
    });

    // ---------------------------------------------------- Filtro que envia ao escolher
    var autoenvio = document.querySelector('form[data-autosubmit]');
    if (autoenvio) {
        autoenvio.addEventListener('change', function () { autoenvio.submit(); });
    }

    // ---------------------------------------------------- Busca com sugestões
    var campo = document.getElementById('busca-termo');
    var lista = document.getElementById('busca-sugestoes');
    if (campo && lista) {
        var urlBase = campo.getAttribute('data-url-sugestoes');
        var espera = null;
        var ultimo = '';

        function fechar() {
            lista.hidden = true;
            lista.innerHTML = '';
            campo.setAttribute('aria-expanded', 'false');
        }

        function mostrar(itens) {
            lista.innerHTML = '';
            if (!itens.length) { fechar(); return; }

            itens.forEach(function (f) {
                var li = document.createElement('li');
                li.setAttribute('role', 'option');
                var a = document.createElement('a');
                a.href = '/Filmes/Detalhes/' + encodeURIComponent(f.id);
                var titulo = document.createElement('span');
                titulo.textContent = f.titulo;
                var genero = document.createElement('small');
                genero.textContent = f.genero;
                a.appendChild(titulo);
                a.appendChild(genero);
                li.appendChild(a);
                lista.appendChild(li);
            });
            lista.hidden = false;
            campo.setAttribute('aria-expanded', 'true');
        }

        campo.addEventListener('input', function () {
            var termo = campo.value.trim();
            clearTimeout(espera);
            if (termo.length < 2) { fechar(); return; }

            espera = setTimeout(function () {
                ultimo = termo;
                fetch(urlBase + '?termo=' + encodeURIComponent(termo), { headers: { 'Accept': 'application/json' } })
                    .then(function (r) { return r.ok ? r.json() : []; })
                    .then(function (itens) { if (termo === ultimo) mostrar(itens); })
                    .catch(fechar);
            }, 250);
        });

        campo.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') fechar();
            if (e.key === 'ArrowDown') {
                var primeiro = lista.querySelector('a');
                if (primeiro) { e.preventDefault(); primeiro.focus(); }
            }
        });

        lista.addEventListener('keydown', function (e) {
            var atual = document.activeElement;
            if (e.key === 'ArrowDown' && atual.parentElement.nextElementSibling) {
                e.preventDefault(); atual.parentElement.nextElementSibling.querySelector('a').focus();
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                var anterior = atual.parentElement.previousElementSibling;
                (anterior ? anterior.querySelector('a') : campo).focus();
            } else if (e.key === 'Escape') {
                fechar(); campo.focus();
            }
        });

        document.addEventListener('click', function (e) {
            if (!e.target.closest('.busca')) fechar();
        });
    }
})();

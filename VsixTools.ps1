



<!DOCTYPE html>
<html lang="en" class=" is-copy-enabled">
  <head prefix="og: http://ogp.me/ns# fb: http://ogp.me/ns/fb# object: http://ogp.me/ns/object# article: http://ogp.me/ns/article# profile: http://ogp.me/ns/profile#">
    <meta charset='utf-8'>
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta http-equiv="Content-Language" content="en">
    <meta name="viewport" content="width=1020">
    
    
    <title>vsix-tools/VsixTools.ps1 at master · dwmkerr/vsix-tools</title>
    <link rel="search" type="application/opensearchdescription+xml" href="/opensearch.xml" title="GitHub">
    <link rel="fluid-icon" href="https://github.com/fluidicon.png" title="GitHub">
    <link rel="apple-touch-icon" sizes="57x57" href="/apple-touch-icon-114.png">
    <link rel="apple-touch-icon" sizes="114x114" href="/apple-touch-icon-114.png">
    <link rel="apple-touch-icon" sizes="72x72" href="/apple-touch-icon-144.png">
    <link rel="apple-touch-icon" sizes="144x144" href="/apple-touch-icon-144.png">
    <meta property="fb:app_id" content="1401488693436528">

      <meta content="@github" name="twitter:site" /><meta content="summary" name="twitter:card" /><meta content="dwmkerr/vsix-tools" name="twitter:title" /><meta content="vsix-tools - A set of Powershell functions to help with vsix files." name="twitter:description" /><meta content="https://avatars1.githubusercontent.com/u/1926984?v=3&amp;s=400" name="twitter:image:src" />
      <meta content="GitHub" property="og:site_name" /><meta content="object" property="og:type" /><meta content="https://avatars1.githubusercontent.com/u/1926984?v=3&amp;s=400" property="og:image" /><meta content="dwmkerr/vsix-tools" property="og:title" /><meta content="https://github.com/dwmkerr/vsix-tools" property="og:url" /><meta content="vsix-tools - A set of Powershell functions to help with vsix files." property="og:description" />
      <meta name="browser-stats-url" content="https://api.github.com/_private/browser/stats">
    <meta name="browser-errors-url" content="https://api.github.com/_private/browser/errors">
    <link rel="assets" href="https://assets-cdn.github.com/">
    <link rel="web-socket" href="wss://live.github.com/_sockets/MzU4NjI1NDoyODdhNTllMmQwZWEzZTFjZWU5NzZmYTA4NzBmZWVhYzo3YjgxMDQxNzBlMGY4MDEyMGQ1MWQ5NDAyMTk4NTVhZjEwMmQwMGM3M2NkMzYyNDU2OTc1NjMzYjFjMGE0ZGQy--d4846ecdc7d60ce1cc7646961c8c4e00da55b29e">
    <meta name="pjax-timeout" content="1000">
    <link rel="sudo-modal" href="/sessions/sudo_modal">

    <meta name="msapplication-TileImage" content="/windows-tile.png">
    <meta name="msapplication-TileColor" content="#ffffff">
    <meta name="selected-link" value="repo_source" data-pjax-transient>

    <meta name="google-site-verification" content="KT5gs8h0wvaagLKAVWq8bbeNwnZZK1r1XQysX3xurLU">
    <meta name="google-analytics" content="UA-3769691-2">

<meta content="collector.githubapp.com" name="octolytics-host" /><meta content="github" name="octolytics-app-id" /><meta content="A1E1816F:6921:421E068:5669AE3C" name="octolytics-dimension-request_id" /><meta content="3586254" name="octolytics-actor-id" /><meta content="jzoss" name="octolytics-actor-login" /><meta content="321dc22f141c32edd37761838b53759d89dc525e4333aaaa3b39c5712dca73fa" name="octolytics-actor-hash" />
<meta content="/&lt;user-name&gt;/&lt;repo-name&gt;/blob/show" data-pjax-transient="true" name="analytics-location" />
<meta content="Rails, view, blob#show" data-pjax-transient="true" name="analytics-event" />


  <meta class="js-ga-set" name="dimension1" content="Logged In">



        <meta name="hostname" content="github.com">
    <meta name="user-login" content="jzoss">

        <meta name="expected-hostname" content="github.com">

      <link rel="mask-icon" href="https://assets-cdn.github.com/pinned-octocat.svg" color="#4078c0">
      <link rel="icon" type="image/x-icon" href="https://assets-cdn.github.com/favicon.ico">

    <meta content="d2c36eeadb5abf1fc45ea88de3a289bb9c9e9fb0" name="form-nonce" />

    <link crossorigin="anonymous" href="https://assets-cdn.github.com/assets/github-c4e84b31ad83e3338ffded095ee7f4bd13abea9e4e85427bce41c960343dae2e.css" media="all" rel="stylesheet" />
    <link crossorigin="anonymous" href="https://assets-cdn.github.com/assets/github2-dbb634f9787dac887112e206dd1bc44b0b513564a9ab6a760d2575625ac399e8.css" media="all" rel="stylesheet" />
    
    


    <meta http-equiv="x-pjax-version" content="7232235cce526ab2b2d5a0a11379c3ea">

      
  <meta name="description" content="vsix-tools - A set of Powershell functions to help with vsix files.">
  <meta name="go-import" content="github.com/dwmkerr/vsix-tools git https://github.com/dwmkerr/vsix-tools.git">

  <meta content="1926984" name="octolytics-dimension-user_id" /><meta content="dwmkerr" name="octolytics-dimension-user_login" /><meta content="18132913" name="octolytics-dimension-repository_id" /><meta content="dwmkerr/vsix-tools" name="octolytics-dimension-repository_nwo" /><meta content="true" name="octolytics-dimension-repository_public" /><meta content="false" name="octolytics-dimension-repository_is_fork" /><meta content="18132913" name="octolytics-dimension-repository_network_root_id" /><meta content="dwmkerr/vsix-tools" name="octolytics-dimension-repository_network_root_nwo" />
  <link href="https://github.com/dwmkerr/vsix-tools/commits/master.atom" rel="alternate" title="Recent Commits to vsix-tools:master" type="application/atom+xml">

  </head>


  <body class="logged_in   env-production windows vis-public page-blob">
    <a href="#start-of-content" tabindex="1" class="accessibility-aid js-skip-to-content">Skip to content</a>

    
    
    



      <div class="header header-logged-in true" role="banner">
  <div class="container clearfix">

    <a class="header-logo-invertocat" href="https://github.com/" data-hotkey="g d" aria-label="Homepage" data-ga-click="Header, go to dashboard, icon:logo">
  <span class="mega-octicon octicon-mark-github"></span>
</a>


      <div class="site-search repo-scope js-site-search" role="search">
          <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/dwmkerr/vsix-tools/search" class="js-site-search-form" data-global-search-url="/search" data-repo-search-url="/dwmkerr/vsix-tools/search" method="get"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /></div>
  <label class="js-chromeless-input-container form-control">
    <div class="scope-badge">This repository</div>
    <input type="text"
      class="js-site-search-focus js-site-search-field is-clearable chromeless-input"
      data-hotkey="s"
      name="q"
      placeholder="Search"
      aria-label="Search this repository"
      data-global-scope-placeholder="Search GitHub"
      data-repo-scope-placeholder="Search"
      tabindex="1"
      autocapitalize="off">
  </label>
</form>
      </div>

      <ul class="header-nav left" role="navigation">
        <li class="header-nav-item">
          <a href="/pulls" class="js-selected-navigation-item header-nav-link" data-ga-click="Header, click, Nav menu - item:pulls context:user" data-hotkey="g p" data-selected-links="/pulls /pulls/assigned /pulls/mentioned /pulls">
            Pull requests
</a>        </li>
        <li class="header-nav-item">
          <a href="/issues" class="js-selected-navigation-item header-nav-link" data-ga-click="Header, click, Nav menu - item:issues context:user" data-hotkey="g i" data-selected-links="/issues /issues/assigned /issues/mentioned /issues">
            Issues
</a>        </li>
          <li class="header-nav-item">
            <a class="header-nav-link" href="https://gist.github.com/" data-ga-click="Header, go to gist, text:gist">Gist</a>
          </li>
      </ul>

    
<ul class="header-nav user-nav right" id="user-links">
  <li class="header-nav-item">
      <span class="js-socket-channel js-updatable-content"
        data-channel="notification-changed:jzoss"
        data-url="/notifications/header">
      <a href="/notifications" aria-label="You have no unread notifications" class="header-nav-link notification-indicator tooltipped tooltipped-s" data-ga-click="Header, go to notifications, icon:read" data-hotkey="g n">
          <span class="mail-status all-read"></span>
          <span class="octicon octicon-bell"></span>
</a>  </span>

  </li>

  <li class="header-nav-item dropdown js-menu-container">
    <a class="header-nav-link tooltipped tooltipped-s js-menu-target" href="/new"
       aria-label="Create new…"
       data-ga-click="Header, create new, icon:add">
      <span class="octicon octicon-plus left"></span>
      <span class="dropdown-caret"></span>
    </a>

    <div class="dropdown-menu-content js-menu-content">
      <ul class="dropdown-menu dropdown-menu-sw">
        
<a class="dropdown-item" href="/new" data-ga-click="Header, create new repository">
  New repository
</a>


  <a class="dropdown-item" href="/organizations/new" data-ga-click="Header, create new organization">
    New organization
  </a>



  <div class="dropdown-divider"></div>
  <div class="dropdown-header">
    <span title="dwmkerr/vsix-tools">This repository</span>
  </div>
    <a class="dropdown-item" href="/dwmkerr/vsix-tools/issues/new" data-ga-click="Header, create new issue">
      New issue
    </a>

      </ul>
    </div>
  </li>

  <li class="header-nav-item dropdown js-menu-container">
    <a class="header-nav-link name tooltipped tooltipped-sw js-menu-target" href="/jzoss"
       aria-label="View profile and more"
       data-ga-click="Header, show menu, icon:avatar">
      <img alt="@jzoss" class="avatar" height="20" src="https://avatars2.githubusercontent.com/u/3586254?v=3&amp;s=40" width="20" />
      <span class="dropdown-caret"></span>
    </a>

    <div class="dropdown-menu-content js-menu-content">
      <div class="dropdown-menu  dropdown-menu-sw">
        <div class=" dropdown-header header-nav-current-user css-truncate">
            Signed in as <strong class="css-truncate-target">jzoss</strong>

        </div>


        <div class="dropdown-divider"></div>

          <a class="dropdown-item" href="/jzoss" data-ga-click="Header, go to profile, text:your profile">
            Your profile
          </a>
        <a class="dropdown-item" href="/stars" data-ga-click="Header, go to starred repos, text:your stars">
          Your stars
        </a>
        <a class="dropdown-item" href="/explore" data-ga-click="Header, go to explore, text:explore">
          Explore
        </a>
          <a class="dropdown-item" href="/integrations" data-ga-click="Header, go to integrations, text:integrations">
            Integrations
          </a>
        <a class="dropdown-item" href="https://help.github.com" data-ga-click="Header, go to help, text:help">
          Help
        </a>

          <div class="dropdown-divider"></div>

          <a class="dropdown-item" href="/settings/profile" data-ga-click="Header, go to settings, icon:settings">
            Settings
          </a>

          <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/logout" class="logout-form" data-form-nonce="d2c36eeadb5abf1fc45ea88de3a289bb9c9e9fb0" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="Loa1Rpz0Za+PjWuhkOGHL+L+MzRUAvNcSCAOyF6NqRD3iq/q3Q5xa6Wxq58vk5/CXLlopCN3L22N6kg8M8AsCQ==" /></div>
            <button class="dropdown-item dropdown-signout" data-ga-click="Header, sign out, icon:logout">
              Sign out
            </button>
</form>
      </div>
    </div>
  </li>
</ul>


    
  </div>
</div>

      

      


    <div id="start-of-content" class="accessibility-aid"></div>

      <div id="js-flash-container">
</div>


    <div role="main" class="main-content">
        <div itemscope itemtype="http://schema.org/WebPage">
    <div id="js-repo-pjax-container" class="context-loader-container js-repo-nav-next" data-pjax-container>
      
<div class="pagehead repohead instapaper_ignore readability-menu experiment-repo-nav">
  <div class="container repohead-details-container">

    

<ul class="pagehead-actions">

  <li>
        <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/notifications/subscribe" class="js-social-container" data-autosubmit="true" data-form-nonce="d2c36eeadb5abf1fc45ea88de3a289bb9c9e9fb0" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="uyyTnF5bZZU9sZ+4eWTsK8UhWxpBQ5xEQFeGngK2PwVSEOu4o/m+4MY/+pxg7Y/zjDlKDxQJ4r0pLawosYAs0Q==" /></div>      <input id="repository_id" name="repository_id" type="hidden" value="18132913" />

        <div class="select-menu js-menu-container js-select-menu">
          <a href="/dwmkerr/vsix-tools/subscription"
            class="btn btn-sm btn-with-count select-menu-button js-menu-target" role="button" tabindex="0" aria-haspopup="true"
            data-ga-click="Repository, click Watch settings, action:blob#show">
            <span class="js-select-button">
              <span class="octicon octicon-eye"></span>
              Watch
            </span>
          </a>
          <a class="social-count js-social-count" href="/dwmkerr/vsix-tools/watchers">
            1
          </a>

        <div class="select-menu-modal-holder">
          <div class="select-menu-modal subscription-menu-modal js-menu-content" aria-hidden="true">
            <div class="select-menu-header">
              <span class="octicon octicon-x js-menu-close" role="button" aria-label="Close"></span>
              <span class="select-menu-title">Notifications</span>
            </div>

              <div class="select-menu-list js-navigation-container" role="menu">

                <div class="select-menu-item js-navigation-item selected" role="menuitem" tabindex="0">
                  <span class="select-menu-item-icon octicon octicon-check"></span>
                  <div class="select-menu-item-text">
                    <input checked="checked" id="do_included" name="do" type="radio" value="included" />
                    <span class="select-menu-item-heading">Not watching</span>
                    <span class="description">Be notified when participating or @mentioned.</span>
                    <span class="js-select-button-text hidden-select-button-text">
                      <span class="octicon octicon-eye"></span>
                      Watch
                    </span>
                  </div>
                </div>

                <div class="select-menu-item js-navigation-item " role="menuitem" tabindex="0">
                  <span class="select-menu-item-icon octicon octicon octicon-check"></span>
                  <div class="select-menu-item-text">
                    <input id="do_subscribed" name="do" type="radio" value="subscribed" />
                    <span class="select-menu-item-heading">Watching</span>
                    <span class="description">Be notified of all conversations.</span>
                    <span class="js-select-button-text hidden-select-button-text">
                      <span class="octicon octicon-eye"></span>
                      Unwatch
                    </span>
                  </div>
                </div>

                <div class="select-menu-item js-navigation-item " role="menuitem" tabindex="0">
                  <span class="select-menu-item-icon octicon octicon-check"></span>
                  <div class="select-menu-item-text">
                    <input id="do_ignore" name="do" type="radio" value="ignore" />
                    <span class="select-menu-item-heading">Ignoring</span>
                    <span class="description">Never be notified.</span>
                    <span class="js-select-button-text hidden-select-button-text">
                      <span class="octicon octicon-mute"></span>
                      Stop ignoring
                    </span>
                  </div>
                </div>

              </div>

            </div>
          </div>
        </div>
</form>
  </li>

  <li>
    
  <div class="js-toggler-container js-social-container starring-container ">

    <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/dwmkerr/vsix-tools/unstar" class="js-toggler-form starred js-unstar-button" data-form-nonce="d2c36eeadb5abf1fc45ea88de3a289bb9c9e9fb0" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="4djrIFnhmgYv79BNp2DZNvS+jNN9wtLcbGSezC1bFCUue0SBQeGilfTvkUNI1ELYAIOf1ZeKcOWZyW477li1kw==" /></div>
      <button
        class="btn btn-sm btn-with-count js-toggler-target"
        aria-label="Unstar this repository" title="Unstar dwmkerr/vsix-tools"
        data-ga-click="Repository, click unstar button, action:blob#show; text:Unstar">
        <span class="octicon octicon-star"></span>
        Unstar
      </button>
        <a class="social-count js-social-count" href="/dwmkerr/vsix-tools/stargazers">
          1
        </a>
</form>
    <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/dwmkerr/vsix-tools/star" class="js-toggler-form unstarred js-star-button" data-form-nonce="d2c36eeadb5abf1fc45ea88de3a289bb9c9e9fb0" data-remote="true" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="/w4KZuFaFtegkEJt5044OSDotxmP3vO5OdvOyEPzhPoETGFUq26WTlSteqA/lF34hPHKjzCmZ41KKhI4WfEJUA==" /></div>
      <button
        class="btn btn-sm btn-with-count js-toggler-target"
        aria-label="Star this repository" title="Star dwmkerr/vsix-tools"
        data-ga-click="Repository, click star button, action:blob#show; text:Star">
        <span class="octicon octicon-star"></span>
        Star
      </button>
        <a class="social-count js-social-count" href="/dwmkerr/vsix-tools/stargazers">
          1
        </a>
</form>  </div>

  </li>

  <li>
          <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/dwmkerr/vsix-tools/fork" class="btn-with-count" data-form-nonce="d2c36eeadb5abf1fc45ea88de3a289bb9c9e9fb0" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="Yd5kv5tWUtlH4NeRyeYTtIABw1jDClt5qTo/0pwXKFTPA6EIGafwiqpPibkKxkAUZmSzR6hUElrTHGIq62CzPQ==" /></div>
            <button
                type="submit"
                class="btn btn-sm btn-with-count"
                data-ga-click="Repository, show fork modal, action:blob#show; text:Fork"
                title="Fork your own copy of dwmkerr/vsix-tools to your account"
                aria-label="Fork your own copy of dwmkerr/vsix-tools to your account">
              <span class="octicon octicon-repo-forked"></span>
              Fork
            </button>
</form>
    <a href="/dwmkerr/vsix-tools/network" class="social-count">
      2
    </a>
  </li>
</ul>

    <h1 itemscope itemtype="http://data-vocabulary.org/Breadcrumb" class="entry-title public ">
  <span class="octicon octicon-repo"></span>
  <span class="author"><a href="/dwmkerr" class="url fn" itemprop="url" rel="author"><span itemprop="title">dwmkerr</span></a></span><!--
--><span class="path-divider">/</span><!--
--><strong><a href="/dwmkerr/vsix-tools" data-pjax="#js-repo-pjax-container">vsix-tools</a></strong>

  <span class="page-context-loader">
    <img alt="" height="16" src="https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif" width="16" />
  </span>

</h1>

  </div>
  <div class="container">
    
<nav class="reponav js-repo-nav js-sidenav-container-pjax js-octicon-loaders"
     role="navigation"
     data-pjax="#js-repo-pjax-container">

  <a href="/dwmkerr/vsix-tools" aria-label="Code" aria-selected="true" class="js-selected-navigation-item selected reponav-item" data-hotkey="g c" data-selected-links="repo_source repo_downloads repo_commits repo_releases repo_tags repo_branches /dwmkerr/vsix-tools">
    <span class="octicon octicon-code"></span>
    Code
</a>
    <a href="/dwmkerr/vsix-tools/issues" class="js-selected-navigation-item reponav-item" data-hotkey="g i" data-selected-links="repo_issues repo_labels repo_milestones /dwmkerr/vsix-tools/issues">
      <span class="octicon octicon-issue-opened"></span>
      Issues
      <span class="counter">0</span>
</a>
  <a href="/dwmkerr/vsix-tools/pulls" class="js-selected-navigation-item reponav-item" data-hotkey="g p" data-selected-links="repo_pulls /dwmkerr/vsix-tools/pulls">
    <span class="octicon octicon-git-pull-request"></span>
    Pull requests
    <span class="counter">0</span>
</a>
    <a href="/dwmkerr/vsix-tools/wiki" class="js-selected-navigation-item reponav-item" data-hotkey="g w" data-selected-links="repo_wiki /dwmkerr/vsix-tools/wiki">
      <span class="octicon octicon-book"></span>
      Wiki
</a>
  <a href="/dwmkerr/vsix-tools/pulse" class="js-selected-navigation-item reponav-item" data-selected-links="pulse /dwmkerr/vsix-tools/pulse">
    <span class="octicon octicon-pulse"></span>
    Pulse
</a>
  <a href="/dwmkerr/vsix-tools/graphs" class="js-selected-navigation-item reponav-item" data-selected-links="repo_graphs repo_contributors /dwmkerr/vsix-tools/graphs">
    <span class="octicon octicon-graph"></span>
    Graphs
</a>

</nav>

  </div>
</div>

<div class="container new-discussion-timeline experiment-repo-nav">
  <div class="repository-content">

    

<a href="/dwmkerr/vsix-tools/blob/4eb84af91dde121b542268a37dcf30dca3d167f5/VsixTools.ps1" class="hidden js-permalink-shortcut" data-hotkey="y">Permalink</a>

<!-- blob contrib key: blob_contributors:v21:f0afc544349ffc63242a9bde5d62256a -->

<div class="file-navigation js-zeroclipboard-container">
  
<div class="select-menu js-menu-container js-select-menu left">
  <button class="btn btn-sm select-menu-button js-menu-target css-truncate" data-hotkey="w"
    title="master"
    type="button" aria-label="Switch branches or tags" tabindex="0" aria-haspopup="true">
    <i>Branch:</i>
    <span class="js-select-button css-truncate-target">master</span>
  </button>

  <div class="select-menu-modal-holder js-menu-content js-navigation-container" data-pjax aria-hidden="true">

    <div class="select-menu-modal">
      <div class="select-menu-header">
        <span class="octicon octicon-x js-menu-close" role="button" aria-label="Close"></span>
        <span class="select-menu-title">Switch branches/tags</span>
      </div>

      <div class="select-menu-filters">
        <div class="select-menu-text-filter">
          <input type="text" aria-label="Filter branches/tags" id="context-commitish-filter-field" class="js-filterable-field js-navigation-enable" placeholder="Filter branches/tags">
        </div>
        <div class="select-menu-tabs">
          <ul>
            <li class="select-menu-tab">
              <a href="#" data-tab-filter="branches" data-filter-placeholder="Filter branches/tags" class="js-select-menu-tab" role="tab">Branches</a>
            </li>
            <li class="select-menu-tab">
              <a href="#" data-tab-filter="tags" data-filter-placeholder="Find a tag…" class="js-select-menu-tab" role="tab">Tags</a>
            </li>
          </ul>
        </div>
      </div>

      <div class="select-menu-list select-menu-tab-bucket js-select-menu-tab-bucket" data-tab-filter="branches" role="menu">

        <div data-filterable-for="context-commitish-filter-field" data-filterable-type="substring">


            <a class="select-menu-item js-navigation-item js-navigation-open selected"
               href="/dwmkerr/vsix-tools/blob/master/VsixTools.ps1"
               data-name="master"
               data-skip-pjax="true"
               rel="nofollow">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <span class="select-menu-item-text css-truncate-target" title="master">
                master
              </span>
            </a>
        </div>

          <div class="select-menu-no-results">Nothing to show</div>
      </div>

      <div class="select-menu-list select-menu-tab-bucket js-select-menu-tab-bucket" data-tab-filter="tags">
        <div data-filterable-for="context-commitish-filter-field" data-filterable-type="substring">


        </div>

        <div class="select-menu-no-results">Nothing to show</div>
      </div>

    </div>
  </div>
</div>

  <div class="btn-group right">
    <a href="/dwmkerr/vsix-tools/find/master"
          class="js-show-file-finder btn btn-sm"
          data-pjax
          data-hotkey="t">
      Find file
    </a>
    <button aria-label="Copy file path to clipboard" class="js-zeroclipboard btn btn-sm zeroclipboard-button tooltipped tooltipped-s" data-copied-hint="Copied!" type="button">Copy path</button>
  </div>
  <div class="breadcrumb js-zeroclipboard-target">
    <span class="repo-root js-repo-root"><span itemscope="" itemtype="http://data-vocabulary.org/Breadcrumb"><a href="/dwmkerr/vsix-tools" class="" data-branch="master" data-pjax="true" itemscope="url"><span itemprop="title">vsix-tools</span></a></span></span><span class="separator">/</span><strong class="final-path">VsixTools.ps1</strong>
  </div>
</div>


  <div class="commit-tease">
      <span class="right">
        <a class="commit-tease-sha" href="/dwmkerr/vsix-tools/commit/89665e470d1b17960106a87c64944abadfed67e5" data-pjax>
          89665e4
        </a>
        <time datetime="2015-01-22T12:15:39Z" is="relative-time">Jan 22, 2015</time>
      </span>
      <div>
        <img alt="@dwmkerr" class="avatar" height="20" src="https://avatars3.githubusercontent.com/u/1926984?v=3&amp;s=40" width="20" />
        <a href="/dwmkerr" class="user-mention" rel="author">dwmkerr</a>
          <a href="/dwmkerr/vsix-tools/commit/89665e470d1b17960106a87c64944abadfed67e5" class="message" data-pjax="true" title="Update VsixTools.ps1">Update VsixTools.ps1</a>
      </div>

    <div class="commit-tease-contributors">
      <a class="muted-link contributors-toggle" href="#blob_contributors_box" rel="facebox">
        <strong>1</strong>
         contributor
      </a>
      
    </div>

    <div id="blob_contributors_box" style="display:none">
      <h2 class="facebox-header" data-facebox-id="facebox-header">Users who have contributed to this file</h2>
      <ul class="facebox-user-list" data-facebox-id="facebox-description">
          <li class="facebox-user-list-item">
            <img alt="@dwmkerr" height="24" src="https://avatars1.githubusercontent.com/u/1926984?v=3&amp;s=48" width="24" />
            <a href="/dwmkerr">dwmkerr</a>
          </li>
      </ul>
    </div>
  </div>

<div class="file">
  <div class="file-header">
  <div class="file-actions">

    <div class="btn-group">
      <a href="/dwmkerr/vsix-tools/raw/master/VsixTools.ps1" class="btn btn-sm " id="raw-url">Raw</a>
        <a href="/dwmkerr/vsix-tools/blame/master/VsixTools.ps1" class="btn btn-sm js-update-url-with-hash">Blame</a>
      <a href="/dwmkerr/vsix-tools/commits/master/VsixTools.ps1" class="btn btn-sm " rel="nofollow">History</a>
    </div>

        <a class="octicon-btn tooltipped tooltipped-nw"
           href="github-windows://openRepo/https://github.com/dwmkerr/vsix-tools?branch=master&amp;filepath=VsixTools.ps1"
           aria-label="Open this file in GitHub Desktop"
           data-ga-click="Repository, open with desktop, type:windows">
            <span class="octicon octicon-device-desktop"></span>
        </a>

        <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/dwmkerr/vsix-tools/edit/master/VsixTools.ps1" class="inline-form js-update-url-with-hash" data-form-nonce="d2c36eeadb5abf1fc45ea88de3a289bb9c9e9fb0" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="ihOvRHXRRVbhajqVG2LzI5ycvOH4tSedwSdkj8WXZJUrsNfiIeq2y0W8ZEJo7J1eUHkXK3csQwr96XSCIGOYYQ==" /></div>
          <button class="octicon-btn tooltipped tooltipped-nw" type="submit"
            aria-label="Fork this project and edit the file" data-hotkey="e" data-disable-with>
            <span class="octicon octicon-pencil"></span>
          </button>
</form>        <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="/dwmkerr/vsix-tools/delete/master/VsixTools.ps1" class="inline-form" data-form-nonce="d2c36eeadb5abf1fc45ea88de3a289bb9c9e9fb0" method="post"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /><input name="authenticity_token" type="hidden" value="XEGy6Jj+GUVpFYFTftzEm2x8+S3lluQPicrHXFMoyaP8VE7ENLhptI72sIFpLZ/0sQY1f8BWcsRC8+S9RlHczw==" /></div>
          <button class="octicon-btn octicon-btn-danger tooltipped tooltipped-nw" type="submit"
            aria-label="Fork this project and delete the file" data-disable-with>
            <span class="octicon octicon-trashcan"></span>
          </button>
</form>  </div>

  <div class="file-info">
      257 lines (212 sloc)
      <span class="file-info-divider"></span>
    9.61 KB
  </div>
</div>

  

  <div class="blob-wrapper data type-powershell">
      <table class="highlight tab-size js-file-line-container" data-tab-size="8">
      <tr>
        <td id="L1" class="blob-num js-line-number" data-line-number="1"></td>
        <td id="LC1" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># VsixTools</span></td>
      </tr>
      <tr>
        <td id="L2" class="blob-num js-line-number" data-line-number="2"></td>
        <td id="LC2" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># Version 1.0</span></td>
      </tr>
      <tr>
        <td id="L3" class="blob-num js-line-number" data-line-number="3"></td>
        <td id="LC3" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># Copyright (c) Dave Kerr 2014</span></td>
      </tr>
      <tr>
        <td id="L4" class="blob-num js-line-number" data-line-number="4"></td>
        <td id="LC4" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L5" class="blob-num js-line-number" data-line-number="5"></td>
        <td id="LC5" class="blob-code blob-code-inner js-file-line"><span class="pl-e">[Reflection.Assembly]</span>::LoadWithPartialName<span class="pl-k">(</span> <span class="pl-s">&quot;System.IO.Compression.FileSystem&quot;</span> <span class="pl-k">)</span> <span class="pl-k">|</span> <span class="pl-c1">Out-Null</span></td>
      </tr>
      <tr>
        <td id="L6" class="blob-num js-line-number" data-line-number="6"></td>
        <td id="LC6" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L7" class="blob-num js-line-number" data-line-number="7"></td>
        <td id="LC7" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># Unzips a zip file at $path to the folder $destination.</span></td>
      </tr>
      <tr>
        <td id="L8" class="blob-num js-line-number" data-line-number="8"></td>
        <td id="LC8" class="blob-code blob-code-inner js-file-line"><span class="pl-k">function</span> <span class="pl-en">Unzip</span>(<span class="pl-k">$</span><span class="pl-smi">path</span><span class="pl-k">,</span> <span class="pl-k">$</span><span class="pl-smi">destination</span>)</td>
      </tr>
      <tr>
        <td id="L9" class="blob-num js-line-number" data-line-number="9"></td>
        <td id="LC9" class="blob-code blob-code-inner js-file-line">{</td>
      </tr>
      <tr>
        <td id="L10" class="blob-num js-line-number" data-line-number="10"></td>
        <td id="LC10" class="blob-code blob-code-inner js-file-line">    <span class="pl-e">[System.IO.Compression.ZipFile]</span>::ExtractToDirectory<span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">path</span><span class="pl-k">,</span> <span class="pl-k">$</span><span class="pl-smi">destination</span><span class="pl-k">)</span> <span class="pl-k">|</span> <span class="pl-c1">Out-Null</span></td>
      </tr>
      <tr>
        <td id="L11" class="blob-num js-line-number" data-line-number="11"></td>
        <td id="LC11" class="blob-code blob-code-inner js-file-line">}</td>
      </tr>
      <tr>
        <td id="L12" class="blob-num js-line-number" data-line-number="12"></td>
        <td id="LC12" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L13" class="blob-num js-line-number" data-line-number="13"></td>
        <td id="LC13" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># Given a path such as &#39;c:\test.vsix&#39; this function </span></td>
      </tr>
      <tr>
        <td id="L14" class="blob-num js-line-number" data-line-number="14"></td>
        <td id="LC14" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># extracts the contents to c:\test.</span></td>
      </tr>
      <tr>
        <td id="L15" class="blob-num js-line-number" data-line-number="15"></td>
        <td id="LC15" class="blob-code blob-code-inner js-file-line"><span class="pl-k">function</span> <span class="pl-en">ExtractVsixToWorkingFolder</span>(<span class="pl-k">$</span><span class="pl-smi">vsixPath</span>) {</td>
      </tr>
      <tr>
        <td id="L16" class="blob-num js-line-number" data-line-number="16"></td>
        <td id="LC16" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L17" class="blob-num js-line-number" data-line-number="17"></td>
        <td id="LC17" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Create the destination directory.</span></td>
      </tr>
      <tr>
        <td id="L18" class="blob-num js-line-number" data-line-number="18"></td>
        <td id="LC18" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">extractFolderName</span> <span class="pl-k">=</span> <span class="pl-e">[System.Io.Path]</span>::GetFileNameWithoutExtension<span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">vsixPath</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L19" class="blob-num js-line-number" data-line-number="19"></td>
        <td id="LC19" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">extractFolderPath</span> <span class="pl-k">=</span> <span class="pl-k">(</span><span class="pl-c1">Join-Path</span> <span class="pl-k">(</span><span class="pl-c1">Split-Path</span> <span class="pl-k">$</span><span class="pl-smi">vsixPath</span><span class="pl-k">)</span> <span class="pl-k">$</span><span class="pl-smi">extractFolderName</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L20" class="blob-num js-line-number" data-line-number="20"></td>
        <td id="LC20" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L21" class="blob-num js-line-number" data-line-number="21"></td>
        <td id="LC21" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Throw if it already exists.</span></td>
      </tr>
      <tr>
        <td id="L22" class="blob-num js-line-number" data-line-number="22"></td>
        <td id="LC22" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">if</span><span class="pl-k">(</span><span class="pl-c1">Test-Path</span> <span class="pl-k">$</span><span class="pl-smi">extractFolderPath</span><span class="pl-k">)</span> {</td>
      </tr>
      <tr>
        <td id="L23" class="blob-num js-line-number" data-line-number="23"></td>
        <td id="LC23" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">throw</span> <span class="pl-s">&quot;Cannot extract the vsix to folder &#39;<span class="pl-k">$</span><span class="pl-smi">extractFolderPath</span>&#39; as it already exists and might cause data loss.&quot;</span></td>
      </tr>
      <tr>
        <td id="L24" class="blob-num js-line-number" data-line-number="24"></td>
        <td id="LC24" class="blob-code blob-code-inner js-file-line">    }</td>
      </tr>
      <tr>
        <td id="L25" class="blob-num js-line-number" data-line-number="25"></td>
        <td id="LC25" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L26" class="blob-num js-line-number" data-line-number="26"></td>
        <td id="LC26" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Extract the zip to the folder.</span></td>
      </tr>
      <tr>
        <td id="L27" class="blob-num js-line-number" data-line-number="27"></td>
        <td id="LC27" class="blob-code blob-code-inner js-file-line">    Unzip <span class="pl-k">$</span><span class="pl-smi">vsixPath</span> <span class="pl-k">$</span><span class="pl-smi">extractFolderPath</span></td>
      </tr>
      <tr>
        <td id="L28" class="blob-num js-line-number" data-line-number="28"></td>
        <td id="LC28" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L29" class="blob-num js-line-number" data-line-number="29"></td>
        <td id="LC29" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Return the extract folder path, which is essentially our working directory.</span></td>
      </tr>
      <tr>
        <td id="L30" class="blob-num js-line-number" data-line-number="30"></td>
        <td id="LC30" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">return</span> <span class="pl-k">$</span><span class="pl-smi">extractFolderPath</span></td>
      </tr>
      <tr>
        <td id="L31" class="blob-num js-line-number" data-line-number="31"></td>
        <td id="LC31" class="blob-code blob-code-inner js-file-line">}</td>
      </tr>
      <tr>
        <td id="L32" class="blob-num js-line-number" data-line-number="32"></td>
        <td id="LC32" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L33" class="blob-num js-line-number" data-line-number="33"></td>
        <td id="LC33" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># Given a path to a vsix, overwrites it with the contents of the s</span></td>
      </tr>
      <tr>
        <td id="L34" class="blob-num js-line-number" data-line-number="34"></td>
        <td id="LC34" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># associated working folder.</span></td>
      </tr>
      <tr>
        <td id="L35" class="blob-num js-line-number" data-line-number="35"></td>
        <td id="LC35" class="blob-code blob-code-inner js-file-line"><span class="pl-k">function</span> <span class="pl-en">ZipWorkingFolderToVsix</span>(<span class="pl-k">$</span><span class="pl-smi">workingFolder</span><span class="pl-k">,</span> <span class="pl-k">$</span><span class="pl-smi">vsixPath</span>) {</td>
      </tr>
      <tr>
        <td id="L36" class="blob-num js-line-number" data-line-number="36"></td>
        <td id="LC36" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L37" class="blob-num js-line-number" data-line-number="37"></td>
        <td id="LC37" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Delete the vsix (as we will overwrite it).</span></td>
      </tr>
      <tr>
        <td id="L38" class="blob-num js-line-number" data-line-number="38"></td>
        <td id="LC38" class="blob-code blob-code-inner js-file-line">    <span class="pl-c1">Copy-Item</span> <span class="pl-k">$</span><span class="pl-smi">vsixPath</span> <span class="pl-k">-</span>Destination <span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">vsixPath</span> <span class="pl-k">+</span> <span class="pl-s">&quot;.backup&quot;</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L39" class="blob-num js-line-number" data-line-number="39"></td>
        <td id="LC39" class="blob-code blob-code-inner js-file-line">    <span class="pl-c1">Remove-Item</span> <span class="pl-k">$</span><span class="pl-smi">vsixPath</span> <span class="pl-k">-</span>Force</td>
      </tr>
      <tr>
        <td id="L40" class="blob-num js-line-number" data-line-number="40"></td>
        <td id="LC40" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L41" class="blob-num js-line-number" data-line-number="41"></td>
        <td id="LC41" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Note we don&#39;t use the .NET method below - for some reason the package</span></td>
      </tr>
      <tr>
        <td id="L42" class="blob-num js-line-number" data-line-number="42"></td>
        <td id="LC42" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># seems to not have the templates extracted.</span></td>
      </tr>
      <tr>
        <td id="L43" class="blob-num js-line-number" data-line-number="43"></td>
        <td id="LC43" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Zip the working folder up and save it at the vsix path</span></td>
      </tr>
      <tr>
        <td id="L44" class="blob-num js-line-number" data-line-number="44"></td>
        <td id="LC44" class="blob-code blob-code-inner js-file-line">    <span class="pl-c">#[System.IO.Compression.ZipFile]::CreateFromDirectory($workingFolder, $vsixPath)</span></td>
      </tr>
      <tr>
        <td id="L45" class="blob-num js-line-number" data-line-number="45"></td>
        <td id="LC45" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L46" class="blob-num js-line-number" data-line-number="46"></td>
        <td id="LC46" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Remove the working folder.</span></td>
      </tr>
      <tr>
        <td id="L47" class="blob-num js-line-number" data-line-number="47"></td>
        <td id="LC47" class="blob-code blob-code-inner js-file-line">    <span class="pl-c">#Remove-Item $workingFolder -Force -Recurse</span></td>
      </tr>
      <tr>
        <td id="L48" class="blob-num js-line-number" data-line-number="48"></td>
        <td id="LC48" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L49" class="blob-num js-line-number" data-line-number="49"></td>
        <td id="LC49" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">vsixZip</span> <span class="pl-k">=</span> <span class="pl-e">[System.IO.Path]</span>::ChangeExtension<span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">vsixPath</span><span class="pl-k">,</span> <span class="pl-s">&quot;zip&quot;</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L50" class="blob-num js-line-number" data-line-number="50"></td>
        <td id="LC50" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">if</span><span class="pl-k">(</span><span class="pl-k">!</span><span class="pl-k">(</span><span class="pl-c1">test-path</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">vsixZip</span><span class="pl-k">)</span><span class="pl-k">)</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L51" class="blob-num js-line-number" data-line-number="51"></td>
        <td id="LC51" class="blob-code blob-code-inner js-file-line">    {</td>
      </tr>
      <tr>
        <td id="L52" class="blob-num js-line-number" data-line-number="52"></td>
        <td id="LC52" class="blob-code blob-code-inner js-file-line">        <span class="pl-c1">set-content</span> <span class="pl-k">$</span><span class="pl-smi">vsixZip</span> <span class="pl-k">(</span><span class="pl-s">&quot;PK&quot;</span> <span class="pl-k">+</span> <span class="pl-e">[char]</span><span class="pl-c1"><span class="pl-c1">5</span></span> <span class="pl-k">+</span> <span class="pl-e">[char]</span><span class="pl-c1"><span class="pl-c1">6</span></span> <span class="pl-k">+</span> <span class="pl-k">(</span><span class="pl-s">&quot;<span class="pl-k">$(</span><span class="pl-e">[char]</span><span class="pl-c1"><span class="pl-c1">0</span></span><span class="pl-k">)</span>&quot;</span> <span class="pl-k">*</span> <span class="pl-c1"><span class="pl-c1">18</span></span><span class="pl-k">)</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L53" class="blob-num js-line-number" data-line-number="53"></td>
        <td id="LC53" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">(</span>dir <span class="pl-k">$</span><span class="pl-smi">vsixZip</span><span class="pl-k">)</span>.IsReadOnly <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-c1">false</span>    </td>
      </tr>
      <tr>
        <td id="L54" class="blob-num js-line-number" data-line-number="54"></td>
        <td id="LC54" class="blob-code blob-code-inner js-file-line">    }</td>
      </tr>
      <tr>
        <td id="L55" class="blob-num js-line-number" data-line-number="55"></td>
        <td id="LC55" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">shellApplication</span> <span class="pl-k">=</span> <span class="pl-c1">new-object</span> <span class="pl-k">-</span>com shell.application</td>
      </tr>
      <tr>
        <td id="L56" class="blob-num js-line-number" data-line-number="56"></td>
        <td id="LC56" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">zipPackage</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">shellApplication</span><span class="pl-en">.NameSpace</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">vsixZip</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L57" class="blob-num js-line-number" data-line-number="57"></td>
        <td id="LC57" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">items</span> <span class="pl-k">=</span> <span class="pl-c1">Get-ChildItem</span> <span class="pl-k">$</span><span class="pl-smi">workingFolder</span></td>
      </tr>
      <tr>
        <td id="L58" class="blob-num js-line-number" data-line-number="58"></td>
        <td id="LC58" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">foreach</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">file</span> <span class="pl-k">in</span> <span class="pl-k">$</span><span class="pl-smi">items</span><span class="pl-k">)</span> </td>
      </tr>
      <tr>
        <td id="L59" class="blob-num js-line-number" data-line-number="59"></td>
        <td id="LC59" class="blob-code blob-code-inner js-file-line">    { </td>
      </tr>
      <tr>
        <td id="L60" class="blob-num js-line-number" data-line-number="60"></td>
        <td id="LC60" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">zipPackage</span><span class="pl-en">.CopyHere</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">file</span><span class="pl-en">.FullName</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L61" class="blob-num js-line-number" data-line-number="61"></td>
        <td id="LC61" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">do</span> {</td>
      </tr>
      <tr>
        <td id="L62" class="blob-num js-line-number" data-line-number="62"></td>
        <td id="LC62" class="blob-code blob-code-inner js-file-line">            <span class="pl-c1">Start-sleep</span> <span class="pl-c1"><span class="pl-c1">2</span></span></td>
      </tr>
      <tr>
        <td id="L63" class="blob-num js-line-number" data-line-number="63"></td>
        <td id="LC63" class="blob-code blob-code-inner js-file-line">        } <span class="pl-k">until</span> <span class="pl-k">(</span> <span class="pl-k">$</span><span class="pl-smi">zipPackage</span><span class="pl-en">.Items</span><span class="pl-k">()</span> <span class="pl-k">|</span> select {<span class="pl-k">$</span><span class="pl-c1">_</span><span class="pl-en">.Name</span> <span class="pl-k">-eq</span> <span class="pl-k">$</span><span class="pl-smi">file</span><span class="pl-en">.Name</span>} <span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L64" class="blob-num js-line-number" data-line-number="64"></td>
        <td id="LC64" class="blob-code blob-code-inner js-file-line">    }</td>
      </tr>
      <tr>
        <td id="L65" class="blob-num js-line-number" data-line-number="65"></td>
        <td id="LC65" class="blob-code blob-code-inner js-file-line">    <span class="pl-c1">Move-Item</span> <span class="pl-k">$</span><span class="pl-smi">vsixZip</span> <span class="pl-k">-</span>Destination <span class="pl-k">$</span><span class="pl-smi">vsixPath</span> <span class="pl-k">-</span>Force</td>
      </tr>
      <tr>
        <td id="L66" class="blob-num js-line-number" data-line-number="66"></td>
        <td id="LC66" class="blob-code blob-code-inner js-file-line">    <span class="pl-c1">Remove-Item</span> <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-k">-</span>Force <span class="pl-k">-</span>Recurse</td>
      </tr>
      <tr>
        <td id="L67" class="blob-num js-line-number" data-line-number="67"></td>
        <td id="LC67" class="blob-code blob-code-inner js-file-line">}</td>
      </tr>
      <tr>
        <td id="L68" class="blob-num js-line-number" data-line-number="68"></td>
        <td id="LC68" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L69" class="blob-num js-line-number" data-line-number="69"></td>
        <td id="LC69" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># Gets the vsix manifest version. Could be:</span></td>
      </tr>
      <tr>
        <td id="L70" class="blob-num js-line-number" data-line-number="70"></td>
        <td id="LC70" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># 1: Visual Studio 2010</span></td>
      </tr>
      <tr>
        <td id="L71" class="blob-num js-line-number" data-line-number="71"></td>
        <td id="LC71" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># 2: Visual Studio 2012 onwards</span></td>
      </tr>
      <tr>
        <td id="L72" class="blob-num js-line-number" data-line-number="72"></td>
        <td id="LC72" class="blob-code blob-code-inner js-file-line"><span class="pl-k">function</span> <span class="pl-en">GetManifestVersion</span>(<span class="pl-k">$</span><span class="pl-smi">manifestXml</span>) {</td>
      </tr>
      <tr>
        <td id="L73" class="blob-num js-line-number" data-line-number="73"></td>
        <td id="LC73" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L74" class="blob-num js-line-number" data-line-number="74"></td>
        <td id="LC74" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Version 1 if we have a Vsix node with Version attribute = 1.</span></td>
      </tr>
      <tr>
        <td id="L75" class="blob-num js-line-number" data-line-number="75"></td>
        <td id="LC75" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">if</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.DocumentElement.Name</span> <span class="pl-k">-eq</span> <span class="pl-s">&quot;Vsix&quot;</span> <span class="pl-k">-and</span> <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.Vsix.Version</span> <span class="pl-k">-eq</span> <span class="pl-s">&quot;1.0.0&quot;</span><span class="pl-k">)</span> {</td>
      </tr>
      <tr>
        <td id="L76" class="blob-num js-line-number" data-line-number="76"></td>
        <td id="LC76" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">return</span> <span class="pl-c1"><span class="pl-c1">1</span></span><span class="pl-k">;</span></td>
      </tr>
      <tr>
        <td id="L77" class="blob-num js-line-number" data-line-number="77"></td>
        <td id="LC77" class="blob-code blob-code-inner js-file-line">    }</td>
      </tr>
      <tr>
        <td id="L78" class="blob-num js-line-number" data-line-number="78"></td>
        <td id="LC78" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L79" class="blob-num js-line-number" data-line-number="79"></td>
        <td id="LC79" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Version 2 if we have a Package manifest node with Version attribute = 2.</span></td>
      </tr>
      <tr>
        <td id="L80" class="blob-num js-line-number" data-line-number="80"></td>
        <td id="LC80" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">if</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.DocumentElement.Name</span> <span class="pl-k">-eq</span> <span class="pl-s">&quot;PackageManifest&quot;</span> <span class="pl-k">-and</span> <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.PackageManifest.Version</span> <span class="pl-k">-eq</span> <span class="pl-s">&quot;2.0.0&quot;</span><span class="pl-k">)</span> {</td>
      </tr>
      <tr>
        <td id="L81" class="blob-num js-line-number" data-line-number="81"></td>
        <td id="LC81" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">return</span> <span class="pl-c1"><span class="pl-c1">2</span></span><span class="pl-k">;</span></td>
      </tr>
      <tr>
        <td id="L82" class="blob-num js-line-number" data-line-number="82"></td>
        <td id="LC82" class="blob-code blob-code-inner js-file-line">    }</td>
      </tr>
      <tr>
        <td id="L83" class="blob-num js-line-number" data-line-number="83"></td>
        <td id="LC83" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L84" class="blob-num js-line-number" data-line-number="84"></td>
        <td id="LC84" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">throw</span> <span class="pl-s">&quot;Unable to determine the version of the Vsix manifest.&quot;</span></td>
      </tr>
      <tr>
        <td id="L85" class="blob-num js-line-number" data-line-number="85"></td>
        <td id="LC85" class="blob-code blob-code-inner js-file-line">}</td>
      </tr>
      <tr>
        <td id="L86" class="blob-num js-line-number" data-line-number="86"></td>
        <td id="LC86" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L87" class="blob-num js-line-number" data-line-number="87"></td>
        <td id="LC87" class="blob-code blob-code-inner js-file-line"><span class="pl-k">function</span> <span class="pl-en">GetManifestNamespaceManager</span>(<span class="pl-k">$</span><span class="pl-smi">manifestXml</span>) {</td>
      </tr>
      <tr>
        <td id="L88" class="blob-num js-line-number" data-line-number="88"></td>
        <td id="LC88" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">ns</span> <span class="pl-k">=</span> <span class="pl-c1">New-Object</span> System.Xml.XmlNamespaceManager<span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.NameTable</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L89" class="blob-num js-line-number" data-line-number="89"></td>
        <td id="LC89" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">ns</span><span class="pl-en">.AddNamespace</span><span class="pl-k">(</span><span class="pl-s">&quot;ns&quot;</span><span class="pl-k">,</span> <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.DocumentElement.NamespaceURI</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L90" class="blob-num js-line-number" data-line-number="90"></td>
        <td id="LC90" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">return</span> <span class="pl-k">,</span><span class="pl-k">$</span><span class="pl-smi">ns</span></td>
      </tr>
      <tr>
        <td id="L91" class="blob-num js-line-number" data-line-number="91"></td>
        <td id="LC91" class="blob-code blob-code-inner js-file-line">}</td>
      </tr>
      <tr>
        <td id="L92" class="blob-num js-line-number" data-line-number="92"></td>
        <td id="LC92" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L93" class="blob-num js-line-number" data-line-number="93"></td>
        <td id="LC93" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># Sets the version of the vsix.</span></td>
      </tr>
      <tr>
        <td id="L94" class="blob-num js-line-number" data-line-number="94"></td>
        <td id="LC94" class="blob-code blob-code-inner js-file-line"><span class="pl-c"># Version should be a string in the format &quot;a.b&quot; &quot;a.b.c&quot; or &quot;a.b.c.d&quot;</span></td>
      </tr>
      <tr>
        <td id="L95" class="blob-num js-line-number" data-line-number="95"></td>
        <td id="LC95" class="blob-code blob-code-inner js-file-line"><span class="pl-k">function</span> <span class="pl-en">Vsix-SetVersion</span> {</td>
      </tr>
      <tr>
        <td id="L96" class="blob-num js-line-number" data-line-number="96"></td>
        <td id="LC96" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">param</span><span class="pl-k">(</span></td>
      </tr>
      <tr>
        <td id="L97" class="blob-num js-line-number" data-line-number="97"></td>
        <td id="LC97" class="blob-code blob-code-inner js-file-line">       <span class="pl-ent">[Paramete<span class="pl-ent">r</span></span><span class="pl-e">(<span class="pl-e"><span class="pl-e"><span class="pl-c1">Mandatory</span>=<span class="pl-smi">$true</span></span></span>)</span><span class="pl-ent">]</span></td>
      </tr>
      <tr>
        <td id="L98" class="blob-num js-line-number" data-line-number="98"></td>
        <td id="LC98" class="blob-code blob-code-inner js-file-line">       <span class="pl-e">[string]</span><span class="pl-k">$</span><span class="pl-smi">VsixPath</span><span class="pl-k">,</span></td>
      </tr>
      <tr>
        <td id="L99" class="blob-num js-line-number" data-line-number="99"></td>
        <td id="LC99" class="blob-code blob-code-inner js-file-line">       <span class="pl-ent">[Paramete<span class="pl-ent">r</span></span><span class="pl-e">(<span class="pl-e"><span class="pl-e"><span class="pl-c1">Mandatory</span>=<span class="pl-smi">$true</span></span></span>)</span><span class="pl-ent">]</span></td>
      </tr>
      <tr>
        <td id="L100" class="blob-num js-line-number" data-line-number="100"></td>
        <td id="LC100" class="blob-code blob-code-inner js-file-line">       <span class="pl-e">[string]</span><span class="pl-k">$</span><span class="pl-smi">Version</span></td>
      </tr>
      <tr>
        <td id="L101" class="blob-num js-line-number" data-line-number="101"></td>
        <td id="LC101" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L102" class="blob-num js-line-number" data-line-number="102"></td>
        <td id="LC102" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L103" class="blob-num js-line-number" data-line-number="103"></td>
        <td id="LC103" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># First, create the working directory.</span></td>
      </tr>
      <tr>
        <td id="L104" class="blob-num js-line-number" data-line-number="104"></td>
        <td id="LC104" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-k">=</span> ExtractVsixToWorkingFolder <span class="pl-k">$</span><span class="pl-smi">VsixPath</span></td>
      </tr>
      <tr>
        <td id="L105" class="blob-num js-line-number" data-line-number="105"></td>
        <td id="LC105" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L106" class="blob-num js-line-number" data-line-number="106"></td>
        <td id="LC106" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Now load the manifest.</span></td>
      </tr>
      <tr>
        <td id="L107" class="blob-num js-line-number" data-line-number="107"></td>
        <td id="LC107" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestPath</span> <span class="pl-k">=</span> <span class="pl-c1">Join-Path</span> <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-s">&quot;extension.vsixmanifest&quot;</span></td>
      </tr>
      <tr>
        <td id="L108" class="blob-num js-line-number" data-line-number="108"></td>
        <td id="LC108" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestXml</span> <span class="pl-k">=</span> <span class="pl-c1">New-Object</span> XML</td>
      </tr>
      <tr>
        <td id="L109" class="blob-num js-line-number" data-line-number="109"></td>
        <td id="LC109" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.Load</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestPath</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L110" class="blob-num js-line-number" data-line-number="110"></td>
        <td id="LC110" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L111" class="blob-num js-line-number" data-line-number="111"></td>
        <td id="LC111" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Set the package version. The xml structure depends on the manifest version.</span></td>
      </tr>
      <tr>
        <td id="L112" class="blob-num js-line-number" data-line-number="112"></td>
        <td id="LC112" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestVersion</span> <span class="pl-k">=</span> GetManifestVersion<span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L113" class="blob-num js-line-number" data-line-number="113"></td>
        <td id="LC113" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">if</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestVersion</span> <span class="pl-k">-eq</span> <span class="pl-c1"><span class="pl-c1">1</span></span><span class="pl-k">)</span> {</td>
      </tr>
      <tr>
        <td id="L114" class="blob-num js-line-number" data-line-number="114"></td>
        <td id="LC114" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.Vsix.Identifier.Version</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">Version</span></td>
      </tr>
      <tr>
        <td id="L115" class="blob-num js-line-number" data-line-number="115"></td>
        <td id="LC115" class="blob-code blob-code-inner js-file-line">    } <span class="pl-k">elseif</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestVersion</span> <span class="pl-k">-eq</span> <span class="pl-c1"><span class="pl-c1">2</span></span><span class="pl-k">)</span> {</td>
      </tr>
      <tr>
        <td id="L116" class="blob-num js-line-number" data-line-number="116"></td>
        <td id="LC116" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.PackageManifest.Metadata.Identity.Version</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">Version</span></td>
      </tr>
      <tr>
        <td id="L117" class="blob-num js-line-number" data-line-number="117"></td>
        <td id="LC117" class="blob-code blob-code-inner js-file-line">    } <span class="pl-k">else</span> {</td>
      </tr>
      <tr>
        <td id="L118" class="blob-num js-line-number" data-line-number="118"></td>
        <td id="LC118" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">throw</span> <span class="pl-s">&quot;Unsupported manifest version&quot;</span></td>
      </tr>
      <tr>
        <td id="L119" class="blob-num js-line-number" data-line-number="119"></td>
        <td id="LC119" class="blob-code blob-code-inner js-file-line">    }</td>
      </tr>
      <tr>
        <td id="L120" class="blob-num js-line-number" data-line-number="120"></td>
        <td id="LC120" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L121" class="blob-num js-line-number" data-line-number="121"></td>
        <td id="LC121" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Save the manifest.</span></td>
      </tr>
      <tr>
        <td id="L122" class="blob-num js-line-number" data-line-number="122"></td>
        <td id="LC122" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.save</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestPath</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L123" class="blob-num js-line-number" data-line-number="123"></td>
        <td id="LC123" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L124" class="blob-num js-line-number" data-line-number="124"></td>
        <td id="LC124" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Finally, save the updated working folder as the vsix.</span></td>
      </tr>
      <tr>
        <td id="L125" class="blob-num js-line-number" data-line-number="125"></td>
        <td id="LC125" class="blob-code blob-code-inner js-file-line">    ZipWorkingFolderToVsix <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-k">$</span><span class="pl-smi">vsixPath</span></td>
      </tr>
      <tr>
        <td id="L126" class="blob-num js-line-number" data-line-number="126"></td>
        <td id="LC126" class="blob-code blob-code-inner js-file-line">}</td>
      </tr>
      <tr>
        <td id="L127" class="blob-num js-line-number" data-line-number="127"></td>
        <td id="LC127" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L128" class="blob-num js-line-number" data-line-number="128"></td>
        <td id="LC128" class="blob-code blob-code-inner js-file-line"><span class="pl-k">function</span> <span class="pl-en">Vsix-FixInvalidMultipleFiles</span> {</td>
      </tr>
      <tr>
        <td id="L129" class="blob-num js-line-number" data-line-number="129"></td>
        <td id="LC129" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">param</span><span class="pl-k">(</span></td>
      </tr>
      <tr>
        <td id="L130" class="blob-num js-line-number" data-line-number="130"></td>
        <td id="LC130" class="blob-code blob-code-inner js-file-line">       <span class="pl-ent">[Paramete<span class="pl-ent">r</span></span><span class="pl-e">(<span class="pl-e"><span class="pl-e"><span class="pl-c1">Mandatory</span>=<span class="pl-smi">$true</span></span></span>)</span><span class="pl-ent">]</span></td>
      </tr>
      <tr>
        <td id="L131" class="blob-num js-line-number" data-line-number="131"></td>
        <td id="LC131" class="blob-code blob-code-inner js-file-line">       <span class="pl-e">[string]</span><span class="pl-k">$</span><span class="pl-smi">VsixPath</span></td>
      </tr>
      <tr>
        <td id="L132" class="blob-num js-line-number" data-line-number="132"></td>
        <td id="LC132" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L133" class="blob-num js-line-number" data-line-number="133"></td>
        <td id="LC133" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L134" class="blob-num js-line-number" data-line-number="134"></td>
        <td id="LC134" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Folder names need to be more than one letter and have different starting letters and numbers.</span></td>
      </tr>
      <tr>
        <td id="L135" class="blob-num js-line-number" data-line-number="135"></td>
        <td id="LC135" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">folderNames</span> <span class="pl-k">=</span> <span class="pl-k">@(</span><span class="pl-s">&quot;Alpha1&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Bravo&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Charlie&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Delta&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Echo&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Foxtrot&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Golf&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Hotel&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;India&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Juliet&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Kilo&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Lima&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Mike&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;November&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Oscar&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Papa&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Quebec&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Romeo&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Sierra&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Tango&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Uniform&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Victor&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Whiskey&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Xray&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Yankee&quot;</span><span class="pl-k">,</span><span class="pl-s">&quot;Zulu&quot;</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L136" class="blob-num js-line-number" data-line-number="136"></td>
        <td id="LC136" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># $folderNames = @(&quot;F1&quot;,&quot;F2&quot;,&quot;F3&quot;,&quot;F4&quot;)</span></td>
      </tr>
      <tr>
        <td id="L137" class="blob-num js-line-number" data-line-number="137"></td>
        <td id="LC137" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L138" class="blob-num js-line-number" data-line-number="138"></td>
        <td id="LC138" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># The gist is this. Find every zip file in Project Templates, e.g:</span></td>
      </tr>
      <tr>
        <td id="L139" class="blob-num js-line-number" data-line-number="139"></td>
        <td id="LC139" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># ProjectTemplates\CSharp\1033\PlumsProject.zip</span></td>
      </tr>
      <tr>
        <td id="L140" class="blob-num js-line-number" data-line-number="140"></td>
        <td id="LC140" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># ProjectTemplates\CSharp\1033\ApplesProject.zip</span></td>
      </tr>
      <tr>
        <td id="L141" class="blob-num js-line-number" data-line-number="141"></td>
        <td id="LC141" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Then put *each one* into a uniquely named folder by replacing</span></td>
      </tr>
      <tr>
        <td id="L142" class="blob-num js-line-number" data-line-number="142"></td>
        <td id="LC142" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># &#39;project templates&#39; in the path with a new unique id</span></td>
      </tr>
      <tr>
        <td id="L143" class="blob-num js-line-number" data-line-number="143"></td>
        <td id="LC143" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># A\CSharp\1033\PlumsProject.zip</span></td>
      </tr>
      <tr>
        <td id="L144" class="blob-num js-line-number" data-line-number="144"></td>
        <td id="LC144" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># B\CSharp\1033\ApplesProject.zip</span></td>
      </tr>
      <tr>
        <td id="L145" class="blob-num js-line-number" data-line-number="145"></td>
        <td id="LC145" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># don&#39;t use numbers, as the Visual Studio Gallery site fails</span></td>
      </tr>
      <tr>
        <td id="L146" class="blob-num js-line-number" data-line-number="146"></td>
        <td id="LC146" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># if you use 1, 11, 111 etc.</span></td>
      </tr>
      <tr>
        <td id="L147" class="blob-num js-line-number" data-line-number="147"></td>
        <td id="LC147" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L148" class="blob-num js-line-number" data-line-number="148"></td>
        <td id="LC148" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># First, create the working directory.</span></td>
      </tr>
      <tr>
        <td id="L149" class="blob-num js-line-number" data-line-number="149"></td>
        <td id="LC149" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-k">=</span> ExtractVsixToWorkingFolder <span class="pl-k">$</span><span class="pl-smi">VsixPath</span></td>
      </tr>
      <tr>
        <td id="L150" class="blob-num js-line-number" data-line-number="150"></td>
        <td id="LC150" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L151" class="blob-num js-line-number" data-line-number="151"></td>
        <td id="LC151" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Get the zip paths. Also create an array that will store the new project template folders.</span></td>
      </tr>
      <tr>
        <td id="L152" class="blob-num js-line-number" data-line-number="152"></td>
        <td id="LC152" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">projectTemplateFolders</span> <span class="pl-k">=</span> <span class="pl-k">@()</span></td>
      </tr>
      <tr>
        <td id="L153" class="blob-num js-line-number" data-line-number="153"></td>
        <td id="LC153" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">folderNameIndex</span> <span class="pl-k">=</span> <span class="pl-c1"><span class="pl-c1">0</span></span></td>
      </tr>
      <tr>
        <td id="L154" class="blob-num js-line-number" data-line-number="154"></td>
        <td id="LC154" class="blob-code blob-code-inner js-file-line">    <span class="pl-c1">Get-ChildItem</span> <span class="pl-k">-</span>Path <span class="pl-k">(</span><span class="pl-c1">Join-Path</span> <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-s">&#39;.\ProjectTemplates&#39;</span><span class="pl-k">)</span> <span class="pl-k">-</span>Filter <span class="pl-k">*</span>.zip <span class="pl-k">-</span>Recurse <span class="pl-k">|</span> <span class="pl-c1">ForEach-Object</span> {</td>
      </tr>
      <tr>
        <td id="L155" class="blob-num js-line-number" data-line-number="155"></td>
        <td id="LC155" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">from</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-c1">_</span><span class="pl-en">.FullName</span></td>
      </tr>
      <tr>
        <td id="L156" class="blob-num js-line-number" data-line-number="156"></td>
        <td id="LC156" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">newPath</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">from</span><span class="pl-en">.Replace</span><span class="pl-k">(</span><span class="pl-s">&#39;\ProjectTemplates\&#39;</span><span class="pl-k">,</span> <span class="pl-s">&#39;\&#39;</span> <span class="pl-k">+</span> <span class="pl-k">$</span><span class="pl-smi">folderNames</span><span class="pl-e">[</span><span class="pl-k">$</span><span class="pl-smi">folderNameIndex</span><span class="pl-e">]</span> <span class="pl-k">+</span> <span class="pl-s">&#39;\&#39;</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L157" class="blob-num js-line-number" data-line-number="157"></td>
        <td id="LC157" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">projectTemplateFolders</span> <span class="pl-k">+=</span> <span class="pl-k">$</span><span class="pl-smi">folderNames</span><span class="pl-e">[</span><span class="pl-k">$</span><span class="pl-smi">folderNameIndex</span><span class="pl-e">]</span></td>
      </tr>
      <tr>
        <td id="L158" class="blob-num js-line-number" data-line-number="158"></td>
        <td id="LC158" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">folderNameIndex</span><span class="pl-k">++</span></td>
      </tr>
      <tr>
        <td id="L159" class="blob-num js-line-number" data-line-number="159"></td>
        <td id="LC159" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L160" class="blob-num js-line-number" data-line-number="160"></td>
        <td id="LC160" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># Copy the file from the old location to the new one, creating a directory chain as necessary.</span></td>
      </tr>
      <tr>
        <td id="L161" class="blob-num js-line-number" data-line-number="161"></td>
        <td id="LC161" class="blob-code blob-code-inner js-file-line">        <span class="pl-c1">New-Item</span> <span class="pl-k">-</span>ItemType File <span class="pl-k">-</span>Path <span class="pl-k">$</span><span class="pl-smi">newPath</span> <span class="pl-k">-</span>Force</td>
      </tr>
      <tr>
        <td id="L162" class="blob-num js-line-number" data-line-number="162"></td>
        <td id="LC162" class="blob-code blob-code-inner js-file-line">        <span class="pl-c1">Copy-Item</span> <span class="pl-k">$</span><span class="pl-smi">from</span> <span class="pl-k">$</span><span class="pl-smi">newPath</span> <span class="pl-k">-</span>Force</td>
      </tr>
      <tr>
        <td id="L163" class="blob-num js-line-number" data-line-number="163"></td>
        <td id="LC163" class="blob-code blob-code-inner js-file-line">    } <span class="pl-k">|</span> <span class="pl-c1">Out-Null</span></td>
      </tr>
      <tr>
        <td id="L164" class="blob-num js-line-number" data-line-number="164"></td>
        <td id="LC164" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L165" class="blob-num js-line-number" data-line-number="165"></td>
        <td id="LC165" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Delete the project templates folder.</span></td>
      </tr>
      <tr>
        <td id="L166" class="blob-num js-line-number" data-line-number="166"></td>
        <td id="LC166" class="blob-code blob-code-inner js-file-line">    <span class="pl-c1">Remove-Item</span> <span class="pl-k">(</span><span class="pl-c1">Join-Path</span> <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-s">&#39;.\ProjectTemplates&#39;</span><span class="pl-k">)</span> <span class="pl-k">-</span>Force <span class="pl-k">-</span>Recurse</td>
      </tr>
      <tr>
        <td id="L167" class="blob-num js-line-number" data-line-number="167"></td>
        <td id="LC167" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L168" class="blob-num js-line-number" data-line-number="168"></td>
        <td id="LC168" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Now load the manifest.</span></td>
      </tr>
      <tr>
        <td id="L169" class="blob-num js-line-number" data-line-number="169"></td>
        <td id="LC169" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestPath</span> <span class="pl-k">=</span> <span class="pl-c1">Join-Path</span> <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-s">&quot;extension.vsixmanifest&quot;</span></td>
      </tr>
      <tr>
        <td id="L170" class="blob-num js-line-number" data-line-number="170"></td>
        <td id="LC170" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestXml</span> <span class="pl-k">=</span> <span class="pl-c1">New-Object</span> XML</td>
      </tr>
      <tr>
        <td id="L171" class="blob-num js-line-number" data-line-number="171"></td>
        <td id="LC171" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.Load</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestPath</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L172" class="blob-num js-line-number" data-line-number="172"></td>
        <td id="LC172" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L173" class="blob-num js-line-number" data-line-number="173"></td>
        <td id="LC173" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Get the manifest version - this will determine what nodes we need to change to match the</span></td>
      </tr>
      <tr>
        <td id="L174" class="blob-num js-line-number" data-line-number="174"></td>
        <td id="LC174" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># new folder structure.</span></td>
      </tr>
      <tr>
        <td id="L175" class="blob-num js-line-number" data-line-number="175"></td>
        <td id="LC175" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestVersion</span> <span class="pl-k">=</span> GetManifestVersion<span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L176" class="blob-num js-line-number" data-line-number="176"></td>
        <td id="LC176" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">ns</span> <span class="pl-k">=</span> GetManifestNamespaceManager<span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L177" class="blob-num js-line-number" data-line-number="177"></td>
        <td id="LC177" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">if</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestVersion</span> <span class="pl-k">-eq</span> <span class="pl-c1"><span class="pl-c1">1</span></span><span class="pl-k">)</span> {</td>
      </tr>
      <tr>
        <td id="L178" class="blob-num js-line-number" data-line-number="178"></td>
        <td id="LC178" class="blob-code blob-code-inner js-file-line">        </td>
      </tr>
      <tr>
        <td id="L179" class="blob-num js-line-number" data-line-number="179"></td>
        <td id="LC179" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># Manifest v1:</span></td>
      </tr>
      <tr>
        <td id="L180" class="blob-num js-line-number" data-line-number="180"></td>
        <td id="LC180" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># Remove all Vsix/Content/ProjectTemplate nodes and replace with A/B/C etc, e.g.:</span></td>
      </tr>
      <tr>
        <td id="L181" class="blob-num js-line-number" data-line-number="181"></td>
        <td id="LC181" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># &lt;Content&gt;</span></td>
      </tr>
      <tr>
        <td id="L182" class="blob-num js-line-number" data-line-number="182"></td>
        <td id="LC182" class="blob-code blob-code-inner js-file-line">        <span class="pl-c">#   &lt;ProjectTemplate&gt;ProjectTemplates&lt;/ProjectTemplate&gt;</span></td>
      </tr>
      <tr>
        <td id="L183" class="blob-num js-line-number" data-line-number="183"></td>
        <td id="LC183" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># &lt;/Content&gt;</span></td>
      </tr>
      <tr>
        <td id="L184" class="blob-num js-line-number" data-line-number="184"></td>
        <td id="LC184" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># to </span></td>
      </tr>
      <tr>
        <td id="L185" class="blob-num js-line-number" data-line-number="185"></td>
        <td id="LC185" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># &lt;Content&gt;</span></td>
      </tr>
      <tr>
        <td id="L186" class="blob-num js-line-number" data-line-number="186"></td>
        <td id="LC186" class="blob-code blob-code-inner js-file-line">        <span class="pl-c">#   &lt;ProjectTemplate&gt;A&lt;/ProjectTemplate&gt;</span></td>
      </tr>
      <tr>
        <td id="L187" class="blob-num js-line-number" data-line-number="187"></td>
        <td id="LC187" class="blob-code blob-code-inner js-file-line">        <span class="pl-c">#   &lt;ProjectTemplate&gt;B&lt;/ProjectTemplate&gt;</span></td>
      </tr>
      <tr>
        <td id="L188" class="blob-num js-line-number" data-line-number="188"></td>
        <td id="LC188" class="blob-code blob-code-inner js-file-line">        <span class="pl-c">#   &lt;ProjectTemplate&gt;C&lt;/ProjectTemplate&gt;</span></td>
      </tr>
      <tr>
        <td id="L189" class="blob-num js-line-number" data-line-number="189"></td>
        <td id="LC189" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># &lt;/Content&gt;</span></td>
      </tr>
      <tr>
        <td id="L190" class="blob-num js-line-number" data-line-number="190"></td>
        <td id="LC190" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">contentNode</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.Vsix.SelectSingleNode</span><span class="pl-k">(</span><span class="pl-s">&quot;ns:Content&quot;</span><span class="pl-k">,</span> <span class="pl-k">$</span><span class="pl-smi">ns</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L191" class="blob-num js-line-number" data-line-number="191"></td>
        <td id="LC191" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">projectTemplateNode</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.Vsix.Content.SelectSingleNode</span><span class="pl-k">(</span><span class="pl-s">&quot;ns:ProjectTemplate&quot;</span><span class="pl-k">,</span> <span class="pl-k">$</span><span class="pl-smi">ns</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L192" class="blob-num js-line-number" data-line-number="192"></td>
        <td id="LC192" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.Vsix.Content.RemoveChild</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">projectTemplateNode</span><span class="pl-k">)</span> <span class="pl-k">|</span> <span class="pl-c1">Out-Null</span></td>
      </tr>
      <tr>
        <td id="L193" class="blob-num js-line-number" data-line-number="193"></td>
        <td id="LC193" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L194" class="blob-num js-line-number" data-line-number="194"></td>
        <td id="LC194" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">foreach</span> <span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">projectTemplateFolder</span> <span class="pl-k">in</span> <span class="pl-k">$</span><span class="pl-smi">projectTemplateFolders</span><span class="pl-k">)</span> {</td>
      </tr>
      <tr>
        <td id="L195" class="blob-num js-line-number" data-line-number="195"></td>
        <td id="LC195" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">$</span><span class="pl-smi">newnode</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">projectTemplateNode</span><span class="pl-en">.CloneNode</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-c1">true</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L196" class="blob-num js-line-number" data-line-number="196"></td>
        <td id="LC196" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">$</span><span class="pl-smi">newnode</span><span class="pl-en">.InnerText</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">projectTemplateFolder</span></td>
      </tr>
      <tr>
        <td id="L197" class="blob-num js-line-number" data-line-number="197"></td>
        <td id="LC197" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">$</span><span class="pl-smi">contentNode</span><span class="pl-en">.AppendChild</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">newnode</span><span class="pl-k">)</span> <span class="pl-k">|</span> <span class="pl-c1">Out-Null</span></td>
      </tr>
      <tr>
        <td id="L198" class="blob-num js-line-number" data-line-number="198"></td>
        <td id="LC198" class="blob-code blob-code-inner js-file-line">        }    </td>
      </tr>
      <tr>
        <td id="L199" class="blob-num js-line-number" data-line-number="199"></td>
        <td id="LC199" class="blob-code blob-code-inner js-file-line">    } <span class="pl-k">elseif</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestVersion</span> <span class="pl-k">-eq</span> <span class="pl-c1"><span class="pl-c1">2</span></span><span class="pl-k">)</span> {</td>
      </tr>
      <tr>
        <td id="L200" class="blob-num js-line-number" data-line-number="200"></td>
        <td id="LC200" class="blob-code blob-code-inner js-file-line">        </td>
      </tr>
      <tr>
        <td id="L201" class="blob-num js-line-number" data-line-number="201"></td>
        <td id="LC201" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># Manifest v2:</span></td>
      </tr>
      <tr>
        <td id="L202" class="blob-num js-line-number" data-line-number="202"></td>
        <td id="LC202" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># Remove all PackageManifest/Assets/Asset (project templat) nodes and replace with A/B/C etc, e.g.:</span></td>
      </tr>
      <tr>
        <td id="L203" class="blob-num js-line-number" data-line-number="203"></td>
        <td id="LC203" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># &lt;Assets&gt;</span></td>
      </tr>
      <tr>
        <td id="L204" class="blob-num js-line-number" data-line-number="204"></td>
        <td id="LC204" class="blob-code blob-code-inner js-file-line">        <span class="pl-c">#   &lt;Asset Type=&quot;Microsoft.VisualStudio.ProjectTemplate&quot; Path=&quot;ProjectTemplates&quot; /&gt;</span></td>
      </tr>
      <tr>
        <td id="L205" class="blob-num js-line-number" data-line-number="205"></td>
        <td id="LC205" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># &lt;/Assets&gt;</span></td>
      </tr>
      <tr>
        <td id="L206" class="blob-num js-line-number" data-line-number="206"></td>
        <td id="LC206" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># to </span></td>
      </tr>
      <tr>
        <td id="L207" class="blob-num js-line-number" data-line-number="207"></td>
        <td id="LC207" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># &lt;Assets&gt;</span></td>
      </tr>
      <tr>
        <td id="L208" class="blob-num js-line-number" data-line-number="208"></td>
        <td id="LC208" class="blob-code blob-code-inner js-file-line">        <span class="pl-c">#   &lt;Asset Type=&quot;Microsoft.VisualStudio.ProjectTemplate&quot; Path=&quot;A&quot; /&gt;</span></td>
      </tr>
      <tr>
        <td id="L209" class="blob-num js-line-number" data-line-number="209"></td>
        <td id="LC209" class="blob-code blob-code-inner js-file-line">        <span class="pl-c">#   &lt;Asset Type=&quot;Microsoft.VisualStudio.ProjectTemplate&quot; Path=&quot;B&quot; /&gt;</span></td>
      </tr>
      <tr>
        <td id="L210" class="blob-num js-line-number" data-line-number="210"></td>
        <td id="LC210" class="blob-code blob-code-inner js-file-line">        <span class="pl-c">#   &lt;Asset Type=&quot;Microsoft.VisualStudio.ProjectTemplate&quot; Path=&quot;C&quot; /&gt;</span></td>
      </tr>
      <tr>
        <td id="L211" class="blob-num js-line-number" data-line-number="211"></td>
        <td id="LC211" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># &lt;/Assets&gt;</span></td>
      </tr>
      <tr>
        <td id="L212" class="blob-num js-line-number" data-line-number="212"></td>
        <td id="LC212" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L213" class="blob-num js-line-number" data-line-number="213"></td>
        <td id="LC213" class="blob-code blob-code-inner js-file-line">        <span class="pl-c"># Get all the existing project template nodes.</span></td>
      </tr>
      <tr>
        <td id="L214" class="blob-num js-line-number" data-line-number="214"></td>
        <td id="LC214" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">assetsNode</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.PackageManifest.SelectSingleNode</span><span class="pl-k">(</span><span class="pl-s">&quot;ns:Assets&quot;</span><span class="pl-k">,</span> <span class="pl-k">$</span><span class="pl-smi">ns</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L215" class="blob-num js-line-number" data-line-number="215"></td>
        <td id="LC215" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">$</span><span class="pl-smi">projectTemplateNodes</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.PackageManifest.Assets.SelectNodes</span><span class="pl-k">(</span><span class="pl-s">&quot;ns:Asset[@Type=&#39;Microsoft.VisualStudio.ProjectTemplate&#39;]&quot;</span><span class="pl-k">,</span> <span class="pl-k">$</span><span class="pl-smi">ns</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L216" class="blob-num js-line-number" data-line-number="216"></td>
        <td id="LC216" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">foreach</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">projectTemplateNode</span> <span class="pl-k">in</span> <span class="pl-k">$</span><span class="pl-smi">projectTemplateNodes</span><span class="pl-k">)</span> {</td>
      </tr>
      <tr>
        <td id="L217" class="blob-num js-line-number" data-line-number="217"></td>
        <td id="LC217" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.PackageManifest.Assets.RemoveChild</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">projectTemplateNode</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L218" class="blob-num js-line-number" data-line-number="218"></td>
        <td id="LC218" class="blob-code blob-code-inner js-file-line">        }</td>
      </tr>
      <tr>
        <td id="L219" class="blob-num js-line-number" data-line-number="219"></td>
        <td id="LC219" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">foreach</span> <span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">projectTemplateFolder</span> <span class="pl-k">in</span> <span class="pl-k">$</span><span class="pl-smi">projectTemplateFolders</span><span class="pl-k">)</span> {</td>
      </tr>
      <tr>
        <td id="L220" class="blob-num js-line-number" data-line-number="220"></td>
        <td id="LC220" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">$</span><span class="pl-smi">newnode</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">projectTemplateNodes</span><span class="pl-e">[</span><span class="pl-c1"><span class="pl-c1">0</span></span><span class="pl-e">]</span>.CloneNode<span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-c1">true</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L221" class="blob-num js-line-number" data-line-number="221"></td>
        <td id="LC221" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">$</span><span class="pl-smi">newnode</span><span class="pl-en">.Path</span> <span class="pl-k">=</span> <span class="pl-k">$</span><span class="pl-smi">projectTemplateFolder</span></td>
      </tr>
      <tr>
        <td id="L222" class="blob-num js-line-number" data-line-number="222"></td>
        <td id="LC222" class="blob-code blob-code-inner js-file-line">            <span class="pl-k">$</span><span class="pl-smi">assetsNode</span><span class="pl-en">.AppendChild</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">newnode</span><span class="pl-k">)</span> <span class="pl-k">|</span> <span class="pl-c1">Out-Null</span></td>
      </tr>
      <tr>
        <td id="L223" class="blob-num js-line-number" data-line-number="223"></td>
        <td id="LC223" class="blob-code blob-code-inner js-file-line">        }</td>
      </tr>
      <tr>
        <td id="L224" class="blob-num js-line-number" data-line-number="224"></td>
        <td id="LC224" class="blob-code blob-code-inner js-file-line">    } <span class="pl-k">else</span> {</td>
      </tr>
      <tr>
        <td id="L225" class="blob-num js-line-number" data-line-number="225"></td>
        <td id="LC225" class="blob-code blob-code-inner js-file-line">        <span class="pl-k">throw</span> <span class="pl-s">&quot;Unsupported manifest version&quot;</span></td>
      </tr>
      <tr>
        <td id="L226" class="blob-num js-line-number" data-line-number="226"></td>
        <td id="LC226" class="blob-code blob-code-inner js-file-line">    }</td>
      </tr>
      <tr>
        <td id="L227" class="blob-num js-line-number" data-line-number="227"></td>
        <td id="LC227" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L228" class="blob-num js-line-number" data-line-number="228"></td>
        <td id="LC228" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Save the manifest.</span></td>
      </tr>
      <tr>
        <td id="L229" class="blob-num js-line-number" data-line-number="229"></td>
        <td id="LC229" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.save</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestPath</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L230" class="blob-num js-line-number" data-line-number="230"></td>
        <td id="LC230" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L231" class="blob-num js-line-number" data-line-number="231"></td>
        <td id="LC231" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L232" class="blob-num js-line-number" data-line-number="232"></td>
        <td id="LC232" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Finally, save the updated working folder as the vsix.</span></td>
      </tr>
      <tr>
        <td id="L233" class="blob-num js-line-number" data-line-number="233"></td>
        <td id="LC233" class="blob-code blob-code-inner js-file-line">    ZipWorkingFolderToVsix <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-k">$</span><span class="pl-smi">vsixPath</span></td>
      </tr>
      <tr>
        <td id="L234" class="blob-num js-line-number" data-line-number="234"></td>
        <td id="LC234" class="blob-code blob-code-inner js-file-line">}</td>
      </tr>
      <tr>
        <td id="L235" class="blob-num js-line-number" data-line-number="235"></td>
        <td id="LC235" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L236" class="blob-num js-line-number" data-line-number="236"></td>
        <td id="LC236" class="blob-code blob-code-inner js-file-line"><span class="pl-k">function</span> <span class="pl-en">Vsix-GetManifestVersion</span> {</td>
      </tr>
      <tr>
        <td id="L237" class="blob-num js-line-number" data-line-number="237"></td>
        <td id="LC237" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">param</span><span class="pl-k">(</span></td>
      </tr>
      <tr>
        <td id="L238" class="blob-num js-line-number" data-line-number="238"></td>
        <td id="LC238" class="blob-code blob-code-inner js-file-line">       <span class="pl-ent">[Paramete<span class="pl-ent">r</span></span><span class="pl-e">(<span class="pl-e"><span class="pl-e"><span class="pl-c1">Mandatory</span>=<span class="pl-smi">$true</span></span></span>)</span><span class="pl-ent">]</span></td>
      </tr>
      <tr>
        <td id="L239" class="blob-num js-line-number" data-line-number="239"></td>
        <td id="LC239" class="blob-code blob-code-inner js-file-line">       <span class="pl-e">[string]</span><span class="pl-k">$</span><span class="pl-smi">VsixPath</span></td>
      </tr>
      <tr>
        <td id="L240" class="blob-num js-line-number" data-line-number="240"></td>
        <td id="LC240" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L241" class="blob-num js-line-number" data-line-number="241"></td>
        <td id="LC241" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L242" class="blob-num js-line-number" data-line-number="242"></td>
        <td id="LC242" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># First, create the working directory.</span></td>
      </tr>
      <tr>
        <td id="L243" class="blob-num js-line-number" data-line-number="243"></td>
        <td id="LC243" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-k">=</span> ExtractVsixToWorkingFolder <span class="pl-k">$</span><span class="pl-smi">VsixPath</span></td>
      </tr>
      <tr>
        <td id="L244" class="blob-num js-line-number" data-line-number="244"></td>
        <td id="LC244" class="blob-code blob-code-inner js-file-line">    </td>
      </tr>
      <tr>
        <td id="L245" class="blob-num js-line-number" data-line-number="245"></td>
        <td id="LC245" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Now load the manifest.</span></td>
      </tr>
      <tr>
        <td id="L246" class="blob-num js-line-number" data-line-number="246"></td>
        <td id="LC246" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestPath</span> <span class="pl-k">=</span> <span class="pl-c1">Join-Path</span> <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-s">&quot;extension.vsixmanifest&quot;</span></td>
      </tr>
      <tr>
        <td id="L247" class="blob-num js-line-number" data-line-number="247"></td>
        <td id="LC247" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L248" class="blob-num js-line-number" data-line-number="248"></td>
        <td id="LC248" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Get the manifest version.</span></td>
      </tr>
      <tr>
        <td id="L249" class="blob-num js-line-number" data-line-number="249"></td>
        <td id="LC249" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestXml</span> <span class="pl-k">=</span> <span class="pl-c1">New-Object</span> XML</td>
      </tr>
      <tr>
        <td id="L250" class="blob-num js-line-number" data-line-number="250"></td>
        <td id="LC250" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-en">.Load</span><span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestPath</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L251" class="blob-num js-line-number" data-line-number="251"></td>
        <td id="LC251" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">$</span><span class="pl-smi">manifestVersion</span> <span class="pl-k">=</span> GetManifestVersion<span class="pl-k">(</span><span class="pl-k">$</span><span class="pl-smi">manifestXml</span><span class="pl-k">)</span></td>
      </tr>
      <tr>
        <td id="L252" class="blob-num js-line-number" data-line-number="252"></td>
        <td id="LC252" class="blob-code blob-code-inner js-file-line">
</td>
      </tr>
      <tr>
        <td id="L253" class="blob-num js-line-number" data-line-number="253"></td>
        <td id="LC253" class="blob-code blob-code-inner js-file-line">    <span class="pl-c"># Finally, clean up the working folder.</span></td>
      </tr>
      <tr>
        <td id="L254" class="blob-num js-line-number" data-line-number="254"></td>
        <td id="LC254" class="blob-code blob-code-inner js-file-line">    <span class="pl-c1">Remove-Item</span> <span class="pl-k">$</span><span class="pl-smi">workingFolder</span> <span class="pl-k">-</span>Force <span class="pl-k">-</span>Recurse</td>
      </tr>
      <tr>
        <td id="L255" class="blob-num js-line-number" data-line-number="255"></td>
        <td id="LC255" class="blob-code blob-code-inner js-file-line">    <span class="pl-k">return</span> <span class="pl-k">$</span><span class="pl-smi">manifestVersion</span></td>
      </tr>
      <tr>
        <td id="L256" class="blob-num js-line-number" data-line-number="256"></td>
        <td id="LC256" class="blob-code blob-code-inner js-file-line">}</td>
      </tr>
</table>

  </div>

</div>

<a href="#jump-to-line" rel="facebox[.linejump]" data-hotkey="l" style="display:none">Jump to Line</a>
<div id="jump-to-line" style="display:none">
  <!-- </textarea> --><!-- '"` --><form accept-charset="UTF-8" action="" class="js-jump-to-line-form" method="get"><div style="margin:0;padding:0;display:inline"><input name="utf8" type="hidden" value="&#x2713;" /></div>
    <input class="linejump-input js-jump-to-line-field" type="text" placeholder="Jump to line&hellip;" aria-label="Jump to line" autofocus>
    <button type="submit" class="btn">Go</button>
</form></div>

  </div>
  <div class="modal-backdrop"></div>
</div>

    </div>
  </div>

    </div>

        <div class="container">
  <div class="site-footer" role="contentinfo">
    <ul class="site-footer-links right">
        <li><a href="https://status.github.com/" data-ga-click="Footer, go to status, text:status">Status</a></li>
      <li><a href="https://developer.github.com" data-ga-click="Footer, go to api, text:api">API</a></li>
      <li><a href="https://training.github.com" data-ga-click="Footer, go to training, text:training">Training</a></li>
      <li><a href="https://shop.github.com" data-ga-click="Footer, go to shop, text:shop">Shop</a></li>
        <li><a href="https://github.com/blog" data-ga-click="Footer, go to blog, text:blog">Blog</a></li>
        <li><a href="https://github.com/about" data-ga-click="Footer, go to about, text:about">About</a></li>
        <li><a href="https://github.com/pricing" data-ga-click="Footer, go to pricing, text:pricing">Pricing</a></li>

    </ul>

    <a href="https://github.com" aria-label="Homepage">
      <span class="mega-octicon octicon-mark-github" title="GitHub"></span>
</a>
    <ul class="site-footer-links">
      <li>&copy; 2015 <span title="0.13902s from github-fe130-cp1-prd.iad.github.net">GitHub</span>, Inc.</li>
        <li><a href="https://github.com/site/terms" data-ga-click="Footer, go to terms, text:terms">Terms</a></li>
        <li><a href="https://github.com/site/privacy" data-ga-click="Footer, go to privacy, text:privacy">Privacy</a></li>
        <li><a href="https://github.com/security" data-ga-click="Footer, go to security, text:security">Security</a></li>
        <li><a href="https://github.com/contact" data-ga-click="Footer, go to contact, text:contact">Contact</a></li>
        <li><a href="https://help.github.com" data-ga-click="Footer, go to help, text:help">Help</a></li>
    </ul>
  </div>
</div>



    
    
    

    <div id="ajax-error-message" class="flash flash-error">
      <span class="octicon octicon-alert"></span>
      <button type="button" class="flash-close js-flash-close js-ajax-error-dismiss" aria-label="Dismiss error">
        <span class="octicon octicon-x"></span>
      </button>
      Something went wrong with that request. Please try again.
    </div>


      <script crossorigin="anonymous" src="https://assets-cdn.github.com/assets/frameworks-b7c9523d93e6cd0235a0a8b7d1a691f7909d3834cd9c9caa7b1674ba508b644c.js"></script>
      <script async="async" crossorigin="anonymous" src="https://assets-cdn.github.com/assets/github-b42430c121e963c7370a99d029c78f3c2fdc9b75b73adf817f07802e33988e81.js"></script>
      
      
      
    <div class="js-stale-session-flash stale-session-flash flash flash-warn flash-banner hidden">
      <span class="octicon octicon-alert"></span>
      <span class="signed-in-tab-flash">You signed in with another tab or window. <a href="">Reload</a> to refresh your session.</span>
      <span class="signed-out-tab-flash">You signed out in another tab or window. <a href="">Reload</a> to refresh your session.</span>
    </div>
  </body>
</html>


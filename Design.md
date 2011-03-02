Something about Sand Design Document
====================================

Philosophy
==========

Why create this game? Why would someone want to play it?
----------------------------------------------

* team play (it's nice to play with your friends!)
* rts-like (with each team member controlling a single unit)

What is the immediate and long-term projected socio-cultural impact of this project?
--------------------------------------------------

* giving people customization over their character for the team's gain (not for personal gain)
* impossible to win solo; victory depends on all three players on a team playing together

Are there any previous games in this genre?
------------------------------

alien swarm, etc.

What is this game's target audience?
------------------------

fans of competitive team play; mostly "gamers", not so much in terms of "casual" gamers

Team-based Play
---------------

(move this elsewhere)
Our game encourages team-based play as much as possible, by dividing the abilities of each class sufficiently to  make it impossible to play without three cooperative players on your team.

Common Questions
================

What is the game?
-----------------

It’s a 3v3 abstract top-down map-conquest game!

Where does the game take place?
-------------------------------

The game takes place on an abstract world with fixed obstacles.

What do I control?
------------------

You control a tank, with a set of tools.

How many characters do I control?
---------------------------------

Each player controls one character at a time.

What is the main focus?
-----------------------

Filling the map with sand, and then (based on the outcome of the sand battle, winners will be offense, losers will be defense) stunning the other team.

What's different?
-----------------

It's a 3rd-person shooter with no death, instead primarily focusing on the creation and destruction of inanimate particles of sand.

Features
========

General Features
----------------

* Top-down view of the game world
* Vector style

Multiplayer Features
--------------------

* 3v3 multiplayer
* LAN only
* Zero-configuration game setup

Gameplay
--------

The Game World
==============

Overview
--------

World Feature x
---------------

The Physical World
------------------

Rendering System
----------------

Camera
------

Game Engine
-----------

Lighting Models
---------------

World Layout
------------

Characters
==========

Overview
--------

Creating a Character
--------------------

Enemies and Monsters
--------------------

User Interface
==============

Overview
--------

User Interface Detail x
-----------------------

Sound
=====

Overview
--------

Red Book Audio
--------------

3D Sound
--------

Sound Design
------------

Multiplayer
===========

Overview
--------

Max Players
-----------

The maximum number of players in a single game of NEED-A-NAME is 6, as each game is comprised of two teams of three players each.

Servers
-------

The game is initially peer-to-peer; as players start the game, Bonjour is used for discovery, and all players become interconnected. This peer-to-peer mesh is used as a side-channel throughout the game, but is primarily used for game setup so that no one needs to be conscious of networking or the addresses of their teammates.

Once all players have connected, the player with the most powerful machine will silently spawn a server, and the game will proceed in an ordinary client-server manner.

Customization
-------------

Internet
--------

This game will not work over the internet. For simplicity, it will be limited to LAN games.

Persistence
-----------

The world is not persistent. Each match will use either a predefined or procedurally generated level which will persist only for the duration of the match.

Saving and Loading
------------------

Players will not be able to save or load multiplayer games; matches are relatively short; as such, being able to do so makes little sense.

Character Rendering
===================

Overview
--------

Character Rendering Detail x
----------------------------

World Editing
=============

Overview
--------

World Editing Detail x
----------------------


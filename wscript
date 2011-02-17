#!/usr/bin/env python

from os import system

def options(opt):
    opt.tool_options('compiler_cxx compiler_c')

def configure(conf):
    conf.check_tool('compiler_cxx compiler_c')

def build(bld):
    bld.add_subdirs("src")
    bld.add_group()

def run(ctx):
    if system("./waf build") is 0:
        system("./build/src/game")

def db(ctx):
    if system("./waf build") is 0:
        system("gdb ./build/src/game")

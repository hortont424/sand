/*
 * Copyright 2011 Tim Horton and Pete Schirmer. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL ANY AUTHORS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include "text.h"

#include <cstdlib>
#include <iostream>
#include <SOIL.h>

int textWidth(const char * text, const char * fontFace, int fontSize)
{
    static cairo_t * cr = NULL;
    cairo_text_extents_t extents;

    if(!cr)
    {
        unsigned char * buffer = (unsigned char *)calloc(4, sizeof(unsigned char));
        cairo_surface_t * surf = cairo_image_surface_create_for_data(buffer, CAIRO_FORMAT_ARGB32, 1, 1, 4);
        cr = cairo_create(surf);
    }

    cairo_select_font_face(cr, fontFace, CAIRO_FONT_SLANT_NORMAL, CAIRO_FONT_WEIGHT_BOLD);
    cairo_set_font_size(cr, fontSize);

    cairo_text_extents(cr, text, &extents);

    return extents.width;
}

Text::Text(const char * text) : Actor::Actor()
{
    this->text = text;

    SetSize(textWidth(text, "Bitstream Vera Sans", 24) + 5, 24 + 5);

    cairo_t * cr;
    unsigned char * buffer;
    cairo_surface_t * surf;
    cairo_font_options_t * cfo;

    buffer = (unsigned char *)calloc(GetW() * GetH() * 4, sizeof(unsigned char));
    surf = cairo_image_surface_create_for_data(buffer, CAIRO_FORMAT_ARGB32, GetW(), GetH(), 4 * GetW());
    cr = cairo_create(surf);

    cfo = cairo_font_options_create();
    cairo_font_options_set_antialias(cfo, CAIRO_ANTIALIAS_GRAY);

    cairo_set_source_rgba(cr, 1.0, 1.0, 1.0, 1.0);
    cairo_move_to(cr, 0, 24);
    cairo_select_font_face(cr, "Bitstream Vera Sans", CAIRO_FONT_SLANT_NORMAL, CAIRO_FONT_WEIGHT_BOLD);
    cairo_set_font_options(cr, cfo);
    cairo_set_font_size(cr, 24);
    cairo_show_text(cr, text);
    cairo_surface_finish(surf);

    glGenTextures(1, &texture_id);
    glBindTexture(GL_TEXTURE_RECTANGLE_ARB, texture_id);
    glTexImage2D(GL_TEXTURE_RECTANGLE_ARB, 0, GL_RGBA, GetW(), GetH(), 0, GL_BGRA, GL_UNSIGNED_BYTE, buffer);
    glBindTexture(GL_TEXTURE_RECTANGLE_ARB, 0);

    free(buffer);
    cairo_surface_destroy(surf);
    cairo_destroy(cr);
}

Text::~Text()
{
    glDeleteTextures(1, &texture_id);
}

void Text::Draw()
{
    glEnable(GL_TEXTURE_RECTANGLE_ARB);

    glBindTexture(GL_TEXTURE_RECTANGLE_ARB, texture_id);

    glColor4f(1.0f, 1.0f, 1.0f, 1.0f);

    glBegin(GL_QUADS);
        glTexCoord2f(0.0f, GetH());
        glVertex2f(0.0f, 0.0f);
        glTexCoord2f(GetW(), GetH());
        glVertex2f(GetW(), 0.0f);
        glTexCoord2f(GetW(), 0.0f);
        glVertex2f(GetW(), GetH());
        glTexCoord2f(0.0f, 0.0f);
        glVertex2f(0.0f, GetH());
    glEnd();

    glDisable(GL_TEXTURE_RECTANGLE_ARB);
}

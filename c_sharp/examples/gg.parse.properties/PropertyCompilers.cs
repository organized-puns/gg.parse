// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.properties
{
    public static class PropertyCompilers
    {
        public static (TypeToPropertyCompiler typeCompiler, AnnotationToPropertyCompiler annotationToPropertyCompiler) CreateCompilers() 
        {
            var annotationCompiler = new AnnotationToPropertyCompiler();
            var typeCompiler = new TypeToPropertyCompiler(annotationCompiler);

            annotationCompiler.TypeBasedCompiler = typeCompiler;

            return (typeCompiler, annotationCompiler);
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

sealed class FXM_2_OBJ {
    struct Vertex {
        public float x, y, z;
        public Vertex( float x, float y, float z ) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
    struct Face {
        public int v0, v1, v2;
        public Face( int v0, int v1, int v2 ) {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
        }
    }
    static void Main() {
        // делает точки вместо запятых в типе float
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo( "en-US" );

        // ищем все fmx файлы в папках и подпапках
        string[] allFilesName = Directory.GetFiles( Directory.GetCurrentDirectory(), "*.fxm", SearchOption.AllDirectories );

        // для каждого fmx файла 
        foreach ( var fxmName in allFilesName ) {
            Console.WriteLine( "Processing " + Path.GetFileNameWithoutExtension( fxmName ) );
            // общие массивы для всех сабмешей
            List<Vertex> vert_list = new List<Vertex>();
            List<Vertex> norm_list = new List<Vertex>();
            List<Vertex> uvst_list = new List<Vertex>();
            // массив сабмешей, внутри массив граней
            List<List<Face>> mesh_list = new List<List<Face>>();
            // имена сабмешей/материалов
            List<string> mtr_list = new List<string>();

            int keyposes = 0;
            int vert_type = 0;

            // открыли fxm файл и прочли данные
            using ( BinaryReader br = new BinaryReader( File.Open( fxmName, FileMode.Open ) ) ) {
                int length = ( int ) br.BaseStream.Length;
                // проверяем версию файла
                int version = br.ReadInt32();
                if ( version != 1 && version != 2 ) {
                    Console.WriteLine( "Invalid version in file " + Path.GetFileNameWithoutExtension( fxmName ) );
                    continue;
                }
                // файл fxm может содержать и данные о скелете модели
                // здесь эти данные пропускаются, но их можно использовать при необходимости
                keyposes = br.ReadInt32();
                for ( int kp = 0; kp < keyposes; kp++ ) {
                    int joint_count = br.ReadInt32();
                    for ( int j = 0; j < joint_count; j++ ) {
                        // пропускаем 8 нулей
                        int skip0 = br.ReadInt32();
                        int skip1 = br.ReadInt32();
                        if ( skip0 != 0 || skip1 != 0 ) { Console.WriteLine( "Invalid keypose zero-block in " + Path.GetFileNameWithoutExtension( fxmName ) ); }
                        // имя
                        string joint_name = ReadName( br );
                        //
                        int nesting_depth = br.ReadInt32();
                        // пропускаем матрицу 4x4
                        for ( int m = 0; m < 16; m++ ) { float val = br.ReadSingle(); }
                    }
                }
                bool header_valid = true;
                for ( int s = 0; s < 36 && header_valid; s++ ) {
                    byte b = br.ReadByte();
                    if ( b != 0 ) { header_valid = false; }
                }
                if ( !header_valid ) {
                    Console.WriteLine( "Invalid header in file " + Path.GetFileNameWithoutExtension( fxmName ) );
                    continue;
                }
                // количество сабмешей
                int submesh_count = br.ReadInt32();
                // для каждой сабмеши
                int fc = 1;
                bool valid = true;
                for ( int subm = 0; subm < submesh_count; subm++ ) {
                    // читаем имя файла текстуры/материала
                    mtr_list.Add( ReadName( br ) );
                    // после имени проверяем и пропускаем 12 нулевых байт
                    for ( int s = 0; s < 12 && valid; s++ ) {
                        byte b = br.ReadByte();
                        if ( b != 0 ) { valid = false; }
                    }
                    // не совсем ясно, что это, но зависит от version
                    int tmp = br.ReadInt32();
                    if ( tmp != 274 && tmp != 530 ) { valid = false; }
                    if ( !valid ) {
                        Console.WriteLine( "Invalid texture suffix in file " + Path.GetFileNameWithoutExtension( fxmName ) );
                        break;
                    }
                    // размер блока на одну вершину
                    vert_type = br.ReadInt32();
                    if ( vert_type != 32 && vert_type != 40 ) {
                        Console.WriteLine( "Invalid vert_type: " + vert_type + " in file " + Path.GetFileNameWithoutExtension( fxmName ) );
                        valid = false;
                        break;
                    }
                    // единичка
                    tmp = br.ReadInt32();
                    if ( tmp != 1 ) { valid = false; }
                    if ( !valid ) {
                        Console.WriteLine( "Invalid block_1 in file " + Path.GetFileNameWithoutExtension( fxmName ) );
                        break;
                    }
                    // количество граней
                    int faces_count = br.ReadInt32();
                    // количество вершин
                    int vertex_count = br.ReadInt32();
                    // Проверяем корректность
                    int mem = faces_count * 2 * 3 + vertex_count * vert_type;
                    var stream = br.BaseStream;
                    long pos = stream.Position;
                    if ( length - pos < mem ) {
                        Console.WriteLine( "Invalid data. Requested: " + mem + ". Pos: " + pos );
                        valid = false;
                        break;
                    }
                    // читаем грани f1 f2 f3
                    List<Face> face_list = new List<Face>();
                    for ( int i = 0; i < faces_count; i++ ) {
                        face_list.Add( new Face( br.ReadInt16() + fc, br.ReadInt16() + fc, br.ReadInt16() + fc ) );
                    }
                    fc += vertex_count;
                    mesh_list.Add( face_list );
                    // читаем информацию по каждой вершине v vn vt
                    for (int i = 0; i < vertex_count; i++) {
                        vert_list.Add( new Vertex( br.ReadSingle(), br.ReadSingle(), br.ReadSingle() ) );
                        norm_list.Add( new Vertex( br.ReadSingle(), br.ReadSingle(), br.ReadSingle() ) );
                        uvst_list.Add( new Vertex( br.ReadSingle(), br.ReadSingle(), 1.0f ) );
                        if ( vert_type == 40 ) {
                            // пока не ясно, что это за данные (возможно, веса), поэтому пропускаем
                            br.ReadSingle();
                            br.ReadSingle();
                        }
                    }
                }
                if ( !valid ) { continue; }
                br.Close();
            }

            if ( keyposes == 0 && vert_type == 32 ) {
                File.Delete( fxmName );
            }

            // записали данные в obj
            using ( StreamWriter sw = new StreamWriter( Directory.GetCurrentDirectory() + "/" + Path.GetFileNameWithoutExtension( fxmName ) + ".obj" ) ) {
                // вычисляем общее количество граней
                int faces_count = 0;
                for ( int g = 0; g < mesh_list.Count; g++ ) {
                    faces_count += mesh_list[ g ].Count;
                }
                // заголовок со статистикой
                sw.WriteLine( "# Wavefront OBJ file" );
                sw.WriteLine( "# num_vertices: " + vert_list.Count );
                sw.WriteLine( "# num_vertex_normals: " + norm_list.Count );
                sw.WriteLine( "# num_uvs: " + uvst_list.Count );
                sw.WriteLine( "# num_faces: " + faces_count );
                sw.WriteLine( "# num_groups: " + mesh_list.Count );
                sw.Write( "\r\n" );
                // ссылка на файл материалов
                sw.WriteLine( "mtllib " + Path.GetFileNameWithoutExtension( fxmName ) + ".mtl" );
                sw.Write( "\r\n" );
                // вершины
                for ( int vi = 0; vi < vert_list.Count; vi++ ) {
                    Vertex v = vert_list[ vi ];
                    sw.WriteLine( String.Format( "v {0:f8} {1:f8} {2:f8}", v.x, v.y, v.z ) );
                }
                sw.Write( "\r\n" );
                // нормали
                for ( int ni = 0; ni < norm_list.Count; ni++ ) {
                    Vertex n = norm_list[ ni ];
                    sw.WriteLine( String.Format( "vn {0:f8} {1:f8} {2:f8}", n.x, n.y, n.z ) );
                }
                sw.Write( "\r\n" );
                // текстурные координаты
                for ( int ti = 0; ti < uvst_list.Count; ti++ ) {
                    Vertex t = uvst_list[ ti ];
                    sw.WriteLine( String.Format( "vt {0:f8} {1:f8}", t.x, 1.0f - t.y ) );
                }
                sw.Write( "\r\n" );
                // сабмеши
                for ( int subm = 0; subm < mesh_list.Count; subm++ ) {
                    sw.WriteLine( "g " + mtr_list[ subm ] );
                    sw.WriteLine( "usemtl " + mtr_list[ subm ] );
                    for ( int fi = 0; fi < mesh_list[ subm ].Count; fi++ ) {
                        Face f = mesh_list[ subm ][ fi ];
                        sw.WriteLine( String.Format( "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", f.v0, f.v1, f.v2 ) );
                    }
                    sw.Write( "\r\n" );
                }
                sw.Close();
            }

            // записали данные в mtl
            List<string> mtr = mtr_list.Distinct<string>().ToList();
            using ( StreamWriter sw = new StreamWriter( Directory.GetCurrentDirectory() + "/" + Path.GetFileNameWithoutExtension( fxmName ) + ".mtl" ) ) {
                // заголовок со статистикой
                sw.WriteLine( "# Wavefront MTL file" );
                sw.WriteLine( "# num_materials: " + mtr.Count );
                sw.Write( "\r\n" );
                // материалы
                for ( int subm = 0; subm < mtr.Count; subm++ ) {
                    sw.WriteLine( "newmtl " + mtr[ subm ] );
                    if ( File.Exists( Directory.GetCurrentDirectory() + "/" + mtr[ subm ] + ".jpg" ) ) {
                        sw.WriteLine( "map_Kd " + mtr[ subm ] + ".jpg" );
                    }
                    else if ( File.Exists( Directory.GetCurrentDirectory() + "/" + mtr[ subm ] + ".tga" ) ) {
                        sw.WriteLine( "map_Kd " + mtr[ subm ] + ".tga" );
                    }
                    else {
                        // Console.WriteLine( "Texture '" + mtr[ subm ] + "' not found in " + Path.GetFileNameWithoutExtension( fxmName ) );
                    }
                    sw.Write( "\r\n" );
                }
                sw.Close();
            }

            // очистка списков
            vert_list.Clear();
            uvst_list.Clear();
            norm_list.Clear();
            mesh_list.Clear();
            mtr_list.Clear();
        }
    }
    // читает строку из потока
    static string ReadName( BinaryReader br ) {
        int name_length = br.ReadInt32();
        if ( name_length == 0 ) {
            Console.WriteLine( "Empty string" );
            return "";
        }
        if ( name_length > 256 ) {
            var stream = br.BaseStream;
            long pos = stream.Position;
            Console.WriteLine( "Long string: " + name_length + ". At pos: " + pos );
        }
        byte[] name = new byte[ name_length ];
        br.Read( name, 0, name_length );
        return System.Text.Encoding.Default.GetString( name );
    }
}

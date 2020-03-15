using System;
using System.IO;

sealed class FXF_2_TXT {

    public static int header = 0;

    static void Main() {

        // делает точки вместо запятых в типе float
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo( "en-US" );

        // ищем все fxf файлы в папках и подпапках
        string[] allFilesName = Directory.GetFiles( Directory.GetCurrentDirectory(), "*.fxf", SearchOption.AllDirectories );

        foreach ( var fxfName in allFilesName ) {
            Console.WriteLine( fxfName );
            // открыли *.fxf файл на чтение
            using ( BinaryReader br = new BinaryReader( File.Open( fxfName, FileMode.Open ) ) ) {
                // открыли все *.txt на запись
                using ( StreamWriter sw = new StreamWriter( Path.GetDirectoryName( fxfName ) + "/" + Path.GetFileNameWithoutExtension( fxfName ) + ".txt" ) ) {
                    StreamWriter sw_mtr = new StreamWriter( Path.GetDirectoryName( fxfName ) + "/" + Path.GetFileNameWithoutExtension( fxfName ) + "_mtr.txt" );
                    StreamWriter sw_tex = new StreamWriter( Path.GetDirectoryName( fxfName ) + "/" + Path.GetFileNameWithoutExtension( fxfName ) + "_tex.txt" );
                    StreamWriter sw_msh = new StreamWriter( Path.GetDirectoryName( fxfName ) + "/" + Path.GetFileNameWithoutExtension( fxfName ) + "_msh.txt" );
                    StreamWriter sw_snd = new StreamWriter( Path.GetDirectoryName( fxfName ) + "/" + Path.GetFileNameWithoutExtension( fxfName ) + "_snd.txt" );
                    StreamWriter sw_mot = new StreamWriter( Path.GetDirectoryName( fxfName ) + "/" + Path.GetFileNameWithoutExtension( fxfName ) + "_mot.txt" );
                    StreamWriter sw_fnt = new StreamWriter( Path.GetDirectoryName( fxfName ) + "/" + Path.GetFileNameWithoutExtension( fxfName ) + "_fnt.txt" );
                    // заголовок - '4' или '7'
                    header = br.ReadInt32();
                    sw.WriteLine( "Version: " + header );
                    // single = 100
                    br.ReadSingle();
                    // 16 нулевых байт
                    for ( int zero = 0; zero < 4; zero++ ) { br.ReadSingle(); }

                    // первый раздел
                    int big_block_count = br.ReadInt32();
                    sw.WriteLine( "==========" );
                    for ( int big_block = 0; big_block < big_block_count; big_block++ ) {
                        // читаем количество блоков
                        int count = br.ReadInt32();
                        // читаем блоки, в них только строка
                        for ( int root = 0; root < count; root++ ) {
                            string name = ReadString( br );
                            sw.WriteLine( root + ". " + name );
                        }
                        sw.WriteLine( "==========" );
                    }

                    int count_entries = br.ReadInt32();
                    sw.WriteLine( "Entries: " + count_entries + "\n" );

                    // второй раздел
                    bool ok = true;
                    int mtr_count = 0;
                    int tex_count = 0;
                    int msh_count = 0;
                    int snd_count = 0;
                    int mot_count = 0;
                    int fnt_count = 0;
                    for ( int i = 0; i < count_entries && ok; i++ ) {
                        int block_type = br.ReadInt32();
                        //Console.WriteLine( "Read " + i + " type: " + block_type );
                        switch ( block_type ) {
                            case 0:
                                ReadTextureBlock( br, sw_tex );
                                tex_count++;
                                break;
                            case 1:
                                ReadMeshBlock( br, sw_msh );
                                msh_count++;
                                break;
                            case 2:
                                ReadSoundBlock( br, sw_snd );
                                snd_count++;
                                break;
                            case 4:
                                ReadMaterialBlock( br, sw_mtr );
                                mtr_count++;
                                break;
                            case 6:
                                ReadMotionBlock( br, sw_mot );
                                mot_count++;
                                break;
                            case 7:
                                ReadFontBlock( br, sw_fnt );
                                fnt_count++;
                                break;
                            default:
                                var stream = br.BaseStream;
                                Console.WriteLine( "Unknown block type: " + block_type + " at: " + stream.Position + " Item: " + i );
                                ok = false;
                                break;
                        }
                    }
                    sw.WriteLine( "Materials: " + mtr_count );
                    sw.WriteLine( "Textures: " + tex_count );
                    sw.WriteLine( "Meshes: " + msh_count );
                    sw.WriteLine( "Sounds: " + snd_count );
                    sw.WriteLine( "Motions: " + mot_count );
                    sw.WriteLine( "Fonts: " + fnt_count );
                    sw.Close();
                    sw_mtr.Close();
                    sw_tex.Close();
                    sw_msh.Close();
                    sw_snd.Close();
                    sw_mot.Close();
                    sw_fnt.Close();
                }
            }
        }
    }

    static string ReadString( BinaryReader br ) {
        int length = br.ReadInt32();
        if ( length == 0 ) { return ""; }
        if ( length > 256 ) {
            var stream = br.BaseStream;
            long pos = stream.Position;
            Console.WriteLine( "Long string: " + length + ". At pos: " + pos );
        }
        byte[] name = new byte[ length ];
        br.Read( name, 0, length );
        return System.Text.Encoding.Default.GetString( name );
    }

    static void ReadTextureBlock( BinaryReader br, StreamWriter sw ) {
        ReadBlock3Name( br, sw );
        int num = 0;
        num = br.ReadInt32(); // всегда 0
        num = br.ReadInt32(); // всегда 0
        num = br.ReadInt32(); // всегда 0
        num = br.ReadInt32(); // 0, 1 или 4
        sw.Write( num + " " );
        num = br.ReadInt32(); // 0, 1 или 4
        sw.Write( num + " " );
        num = br.ReadInt32(); // 0,1,3,4
        sw.WriteLine( num );
        int w = br.ReadInt32();
        int h = br.ReadInt32();
        sw.WriteLine( w + " x " + h );
        sw.WriteLine( "=========="  );
    }

    static void ReadMeshBlock( BinaryReader br, StreamWriter sw ) {
        ReadBlock3Name( br, sw );
        int num = 0;
        num = br.ReadInt32();  // всегда 0
        num = br.ReadInt32();  // всегда 0
        int counter_submeshes = br.ReadInt32();
        sw.WriteLine( "subm: " + counter_submeshes );
        for ( int i = 0; i < counter_submeshes; i++ ) {
            sw.WriteLine( br.ReadUInt32() + " " + br.ReadUInt32() );
            for ( int t = 0; t < 9; t++ ) {
                float v = br.ReadSingle();
                sw.Write( String.Format( "{0:f} ", v ) );
            }
            sw.Write( "\n" );
            sw.WriteLine( String.Format( "{0:f} {1:f}", br.ReadSingle(), br.ReadSingle() ) );
            sw.WriteLine( br.ReadInt32() ); // ссылка на id (но бывает и -1 и 0)
            num = br.ReadInt32(); // всегда 0
            num = br.ReadInt32(); // всегда 0
            sw.WriteLine( "___" );
        }
        num = br.ReadInt32(); // 0 или реже 1
        sw.Write( num + " " );
        num = br.ReadInt32(); // 1 - 286 (чаще 1)
        sw.WriteLine( num );
        // у FBI здесь всегда FF FF FF FF
        if ( header == 7 ) { br.ReadSingle(); }
        sw.WriteLine( "==========" );
    }

    static void ReadSoundBlock( BinaryReader br, StreamWriter sw ) {
        ReadBlock3Name( br, sw );
        int num = 0;
        num = br.ReadInt32(); // всегда 0
        num = br.ReadInt32(); // всегда 0
        num = br.ReadInt32(); // 0,1,3
        sw.Write( num + " " );
        sw.Write( br.ReadSingle() + " " ); // 0.0 - 1.0
        num = br.ReadInt32(); // 0,1,2 (чаще 0)
        sw.WriteLine( num );
        sw.WriteLine( "==========" );
    }

    static void ReadMaterialBlock( BinaryReader br, StreamWriter sw ) {
        ReadBlock3Name( br, sw );
        int num = 0;
        num = br.ReadInt32(); // всегда 0
        num = br.ReadInt32(); // всегда 0
        for ( int i = 0; i < 7; i++ ) { sw.Write( br.ReadSingle() + " " ); }
        sw.WriteLine();
        br.ReadInt32(); // всегда 0
        sw.WriteLine( String.Format( "{0:f} {1:f} {2:f}", br.ReadSingle(), br.ReadSingle(), br.ReadSingle() ) );
        br.ReadInt32(); // всегда 0
        sw.WriteLine( String.Format( "{0:f} {1:f} {2:f}", br.ReadSingle(), br.ReadSingle(), br.ReadSingle() ) );
        br.ReadInt32(); // всегда 0
        sw.WriteLine( String.Format( "{0:f} {1:f} {2:f}", br.ReadSingle(), br.ReadSingle(), br.ReadSingle() ) );
        br.ReadInt32(); // всегда 0
        uint n = br.ReadUInt32(); // 0,1,2 (чаще 1)
        int texture_ref = br.ReadInt32(); // ссылка на id
        sw.WriteLine( texture_ref );
        for ( int i = 0; i < 9 + n; i++ ) { sw.Write( br.ReadInt32() + " " ); }
        sw.WriteLine();
        sw.WriteLine( "==========" );
    }

    static void ReadMotionBlock( BinaryReader br, StreamWriter sw ) {
        ReadBlock3Name( br, sw );
        br.ReadInt32(); // всегда 0
        br.ReadInt32(); // всегда 0
        br.ReadInt32(); // всегда 0
        sw.WriteLine( "==========" );
    }

    static void ReadFontBlock( BinaryReader br, StreamWriter sw ) {
        ReadBlock3Name( br, sw );
        int num = br.ReadInt32(); // всегда 0
        num = br.ReadInt32(); // всегда 0
        // font name
        string name = ReadString( br );
        //  data
        int size = br.ReadInt32();
        for ( int zero = 0; zero < size; zero++ ) { br.ReadByte(); }
        sw.WriteLine( "chars:" + size );
        for ( int zero = 0; zero < 5; zero++ ) {
            sw.Write( br.ReadInt32() + " " );
        }
        sw.WriteLine( br.ReadSingle() );
        sw.WriteLine( br.ReadInt32() + " x " + br.ReadInt32() );
        size = br.ReadInt32();
        sw.WriteLine( "chars:" + size );
        br.ReadSingle(); // всегда 1.0
        for ( int zero = 0; zero < size; zero++ ) { br.ReadByte(); }
        for ( int zero = 0; zero < 8; zero++ ) {
            sw.Write( br.ReadUInt32() + " " );
        }
        sw.WriteLine( "\nData:" );
        for ( int zero = 0; zero < size; zero++ ) {
            sw.WriteLine( String.Format( "{0:f} {1:f} {2:f} {3:f}", br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() ) );
        }
        sw.WriteLine( "==========" );
    }

    static void ReadBlock3Name( BinaryReader br, StreamWriter sw ) {
        int num = br.ReadInt32(); // номер сущности
        int suf = br.ReadInt32(); // номер папки внутри соответствующего типа
        sw.WriteLine( num + " " + suf + ": " + ReadString( br ) );
        string s = ReadString( br );
        if ( s.Length > 0 ) { sw.WriteLine( s ); } // имя файла
        s = ReadString( br );
        if ( s.Length > 0 ) { sw.WriteLine( s ); } // полный путь
    }
}

meta:
  id: csa
  endian: le
  application: Nosferatu The wrath of Malachi / FBI Hostage Rescue
  file-extension: csa

seq:
  - id: header # GEEK
    type: str
    size: 4
    encoding: ASCII

  - id: version # 256
    type: u4
    
  - id: first_file_offset
    type: u4

  - id: game_label # 0 or 1048576
    type: u4

  - id: file_count
    type: u4

  - id: files
    type: file
    repeat: expr
    repeat-expr: file_count

################################
types:

  file:
    seq:
      - id: offset
        type: u4

      - id: size
        type: u4

      - id: file_name
        type: str
        size: 128
        encoding: ASCII

meta:
  id: fxf
  endian: le
  application: Nosferatu The wrath of Malachi / FBI Hostage Rescue
  file-extension: fxf

seq:
  - id: version # 4 or 7
    type: u4
    enum: version_type

  - id: header_suff
    type: f4

  - id: zero_16
    size: 16

  - id: big_folder_block_count
    type: u4
    
  - id: big_blocks
    type: big_folder_block
    repeat: expr
    repeat-expr: big_folder_block_count

  - id: count_entries
    type: u4

  - id: all
    type: 
      switch-on: version
      cases:
        'version_type::v4': all_v4
        'version_type::v7': all_v7
    repeat: expr
    repeat-expr: count_entries

################################
types:

  all_v4:
    seq:
      - id: block_type
        type: u4
        enum: block_type
      - id: block
        type: 
          switch-on: block_type
          cases:
            'block_type::texture':  texture_block  # 0
            'block_type::mesh':     meshv4_block   # 1
            'block_type::sound':    sound_block    # 2
            'block_type::material': material_block # 4
            'block_type::motion':   mot_block      # 6
            'block_type::font':     font_block     # 7

  all_v7:
    seq:
      - id: block_type
        type: u4
        enum: block_type
      - id: block
        type: 
          switch-on: block_type
          cases:
            'block_type::texture':  texture_block  # 0
            'block_type::mesh':     meshv7_block   # 1
            'block_type::sound':    sound_block    # 2
            'block_type::material': material_block # 4
            'block_type::motion':   mot_block      # 6
            'block_type::font':     font_block     # 7

  big_folder_block:
    seq:
      - id: small_folder_block_count
        type: u4
      - id: small_folder_blocks
        type: small_folder_block
        repeat: expr
        repeat-expr: small_folder_block_count

  small_folder_block:
    seq:
      - id: len
        type: u4
      - id: name
        type: str
        size: len
        encoding: ASCII

  str_len:
    seq:
    - id: len
      type: u4
    - id: name
      type: str
      size: len
      encoding: ASCII

  block_3_name:
    seq:
      - id: number
        type: u4
      - id: zero
        size: 4
      - id: name
        type: str_len
        repeat: expr
        repeat-expr: 3

  material_block:
    seq:
      - id: number
        type: u4
      - id: zero
        size: 4
      - id: name
        type: str_len

      - id: us1
        type: u4
        repeat: expr
        repeat-expr: 4

      - id: us2
        type: f4
        repeat: expr
        repeat-expr: 7

      - id: us3
        type: u4
        repeat: expr
        repeat-expr: 5

      - id: floats3
        type: f4
        repeat: expr
        repeat-expr: 3

      - id: us4
        type: u4
        repeat: expr
        repeat-expr: 5
      - id: len
        type: u4
      - id: reference_on_texture
        type: u4

      - id: us5
        type: u4
        repeat: expr
        repeat-expr: 9 + len

  texture_block:
    seq:
     - id: name
       type: block_3_name
     - id: hz
       type: u4
       repeat: expr
       repeat-expr: 6
     - id: height
       type: u4
     - id: width
       type: u4

  mot_block: 
    seq:
      - id: name
        type: block_3_name
      - id: hz
        type: u4
        repeat: expr
        repeat-expr: 3

  sound_block:
    seq:
      - id: name
        type: block_3_name
      - id: hz
        type: u4
        repeat: expr
        repeat-expr: 5

  meshv4_block:
    seq:
      - id: name
        type: block_3_name

      - id: zero1
        type: u4
        
      - id: zero2
        type: u4

      - id: counter_submeshes
        type: u4

      - id: hz
        type: float11
        repeat: expr
        repeat-expr: counter_submeshes

      - id: hz17
        type: u4
      - id: first_number_in_anb_file
        type: u4

  meshv7_block:
    seq:
      - id: name
        type: block_3_name

      - id: zero1
        type: u4
        
      - id: zero2
        type: u4

      - id: counter_submeshes
        type: u4

      - id: hz
        type: float11
        repeat: expr
        repeat-expr: counter_submeshes

      - id: hz17
        type: u4
      - id: first_number_in_anb_file
        type: u4
      - id: hz18
        type: f4

  font_block:
    seq:
      - id: number
        type: u4
      - id: zero
        size: 4
      - id: block_name
        type: str_len
      - id: zero_16
        size: 16
      - id: font_name
        type: str_len
      - id: s1
        type: u4
      - id: data1
        size: s1
      - id: hz1
        size: 32
      - id: s2
        type: u4
      - id: fhz
        type: f4
      - id: data2
        size: s2
      - id: hz2
        size: 32
      - id: fdata
        size: s2*16

  float11:
    seq:
      - id: hz1
        type: u4
      - id: ref_to_material
        type: u4

      - id: hz3
        type: f4
      - id: hz4
        type: f4
      - id: hz5
        type: f4

      - id: hz6
        type: f4
      - id: hz7
        type: f4
      - id: hz8
        type: f4

      - id: hz9
        type: f4
      - id: hz10
        type: f4
      - id: hz11
        type: f4

      - id: hz12
        type: f4
      - id: hz13
        type: f4

      - id: hz17
        type: u4
      - id: hz18
        type: u4
      - id: hz19
        type: u4

#########################
enums:

  block_type:
    0: texture
    1: mesh
    2: sound
    4: material
    6: motion
    7: font

  version_type:
    4: v4
    7: v7


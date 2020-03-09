meta:
  id: fxm
  endian: le
  application: Nosferatu The wrath of Malachi / FBI Hostage Rescue
  file-extension: fxm

seq:
  - id: version # 1 or 2
    type: u4
    
  - id: pose_count
    type: u4
  - id: keyposes
    type: keypose
    repeat: expr
    repeat-expr: pose_count

  - id: zero36
    size: 36

  - id: submesh_count
    type: u4
  - id: submeshes
    type: submesh
    repeat: expr
    repeat-expr: submesh_count

################################
types:

  keypose:
    seq:
      - id: count
        type: u4
      - id: joints
        type: joint
        repeat: expr
        repeat-expr: count

  joint:
    seq:
      - id: zero8
        size: 8
      - id: joint_name
        type: block_name
      - id: nesting_depth
        type: u4
      - id: matrix4x4
        type: matrix4x4

  block_name: 
    seq:
      - id: str_len
        type: u4
      - id: string
        type: str
        size: str_len
        encoding: ascii

  matrix4x4:
    seq:
      - id: row
        type: row
        repeat: expr
        repeat-expr: 4

  row:
    seq:
      - id: seq
        type: f4
        repeat: expr
        repeat-expr: 4

  submesh:
    seq:
      - id: texture_name_length
        type: u4
      - id: texture_name
        type: str
        size: texture_name_length
        encoding: ASCII

      - id: zero12
        size: 12

      - id: unknown # 274 or 530
        type: u4

      - id: vertex_type
        type: u4
        enum: vertex_type

      - id: one
        type: u4

      - id: face_count
        type: u4
    
      - id: vertex_count
        type: u4

      - id: faces
        type: face
        repeat: expr
        repeat-expr: face_count

      - id: vertices
        type: 
          switch-on: vertex_type
          cases:
            'vertex_type::v1': vertex_v1
            'vertex_type::v2': vertex_v2
        repeat: expr
        repeat-expr: vertex_count

  face:
    seq:
      - id: vi0
        type: u2
      - id: vi1
        type: u2
      - id: vi2
        type: u2

  vertex_v1:
    seq:
      - id: x
        type: f4
      - id: y
        type: f4
      - id: z
        type: f4

      - id: vn1
        type: f4
      - id: vn2
        type: f4
      - id: vn3
        type: f4

      - id: u
        type: f4
      - id: v
        type: f4

  vertex_v2:
    seq:
      - id: x
        type: f4
      - id: y
        type: f4
      - id: z
        type: f4

      - id: vn1
        type: f4
      - id: vn2
        type: f4
      - id: vn3
        type: f4

      - id: u
        type: f4
      - id: v
        type: f4

      - id: a
        type: f4
      - id: b
        type: f4

################################
enums:
  vertex_type:
    32: v1
    40: v2
